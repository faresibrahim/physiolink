# Cross-tenant data leak: clinic query filter frozen at startup

**Severity:** Critical (cross-tenant PHI/PII leak)
**Component:** `PhysioLink.Infrastructure` — EF Core global query filter
**Status:** FIXED 2026-08-17 (code + regression test verified locally; deploy pending)
**Reported:** 2026-08-17

> **Fix applied.** The query filter now roots the clinic id on the DbContext instance
> (`Expression.Field(Expression.Constant(this), _currentClinicId)`), resolved once per
> request in the constructor, so EF re-parameterizes it per executing context. Verified
> against the dev DB: the new regression test `ClinicModelCacheIsolationTests` fails on the
> old frozen-constant filter and passes on the fix; all 4 isolation tests green. **Still
> needs deploy to Railway.** After deploy, restart the API and re-check the Bethlehem account.

---

## Summary

Every admin-panel account sees the **same** clinic's data (patients, therapists,
appointments, appointment requests) regardless of which clinic the logged-in user
belongs to. Clinic isolation is effectively not enforced at runtime.

Discovered when a newly-created `ClinicAdmin` for **Bethlehem University Clinic**
logged in and saw **Main Clinic's** records instead of an empty tenant.

## Impact / blast radius

- **Confidentiality:** Any authenticated clinic admin can read another clinic's
  patients, therapists, appointments, and appointment requests. This is patient
  health information — a serious data-protection breach, not a cosmetic bug.
- **Scope:** All `ClinicScopedEntity` types — `Patient`, `Therapist`,
  `Appointment`, `AppointmentSlot`. Anything relying on the global clinic filter.
- **Not affected:** `Exercise` / `ExerciseAssignment` are intentionally a shared
  global catalog (`AuditableEntity`, not clinic-scoped) — that sharing is by design.
- **Writes:** Creation paths stamp `ClinicId` from `GetCurrentClinicId()` at write
  time (e.g. `AdminPatientService.CreateAsync`), so new rows may be written under
  whatever single clinic scope the process resolved — compounding the mixing.

## Reproduction

1. Seed/existing data under **Main Clinic** (`11111111-1111-1111-1111-111111111111`).
2. Create a second clinic (**Bethlehem**, `05e24d15-618c-4d52-9e7f-e1d9166a9c7d`)
   and a `ClinicAdmin` user correctly linked to it.
3. Log into the admin panel as the Bethlehem admin.
4. **Expected:** empty clinic (only the shared exercise catalog visible).
5. **Actual:** Main Clinic's patients, therapists, and appointment requests appear.

### Data verified correct (rules out a data/SQL cause)

```
Email                       user_clinic_id                        clinic_name
buclinic@gmail.com          05e24d15-618c-4d52-9e7f-e1d9166a9c7d   Bethlehem University Clinic
fares.a.ibrahim@gmail.com   11111111-1111-1111-1111-111111111111   Main Clinic
```

The Bethlehem user **is** attached to the Bethlehem clinic, and three distinct
clinics exist. The token is generated from `user.ClinicId`
(`TokenService.GenerateAccessToken`), so the JWT carries the correct clinic.
The leak is therefore in how the filter is *applied*, not in the data or the token.

## Root cause

Clinic scoping is a single EF Core global query filter, built dynamically in
`OnModelCreating`:

`Backend/PhysioLink.Infrastructure/Data/PhysioLinkDbContext.cs` (`BuildClinicScopedFilter`, ~lines 57-73):

```csharp
var getCurrentClinicId = Expression.Call(
    Expression.Constant(_currentClinicService),   // <-- frozen service instance
    typeof(ICurrentClinicService).GetMethod(nameof(ICurrentClinicService.GetCurrentClinicId))!);
var body = Expression.AndAlso(
    Expression.Equal(isDeleted, Expression.Constant(false)),
    Expression.Equal(clinicId, getCurrentClinicId));
```

Two facts combine to defeat isolation:

1. **The filter captures a specific service instance as a literal constant**
   (`Expression.Constant(_currentClinicService)`) instead of resolving the clinic
   per request through the live executing context.

2. **The model is compiled once, at startup, and cached app-wide.** The first
   thing that builds the model is the migration bootstrap, before any HTTP request:

   `Backend/PhysioLink.API/Program.cs` (~lines 142-146):
   ```csharp
   using (var scope = app.Services.CreateScope())
   {
       var db = scope.ServiceProvider.GetRequiredService<PhysioLinkDbContext>();
       await db.Database.MigrateAsync();   // builds + caches the model here
   }
   ```
   There is no `IModelCacheKeyFactory`, so EF Core reuses that one cached model —
   with the startup-scoped clinic expression frozen into it — for every request.

The result: the clinic scope is not re-evaluated per request. All requests resolve
against a single shared clinic scope, so the Bethlehem admin's queries return Main
Clinic rows.

### Why the existing tests miss it

`Backend/PhysioLink.Tests/ClinicIsolationTests.cs` constructs a **fresh**
`DbContextOptions` and a new `PhysioLinkDbContext` per test method, each with its own
`FakeClinicService`. Every test therefore gets a **newly built model** carrying the
right clinic. The bug only manifests when a **single cached model** is reused across
requests for **different** clinics — precisely the production path the tests never
exercise. Green tests here are a false negative.

## Proposed fix

Resolve the clinic id **once per request in the DbContext constructor**, store it in
a field, and have the filter reference that field through the context instance. This
is EF Core's supported multi-tenant pattern: EF re-binds the context reference per
instance, so even with a cached model the parameter value is correct on every request.

Sketch:

```csharp
public class PhysioLinkDbContext : DbContext
{
    private readonly Guid? _currentClinicId;

    public PhysioLinkDbContext(DbContextOptions<PhysioLinkDbContext> options,
                               ICurrentClinicService currentClinicService)
        : base(options)
    {
        // Resolve per-request, at construction, NOT baked into the cached model.
        _currentClinicId = currentClinicService.GetCurrentClinicId();
    }

    // filter references the context field via a this-rooted access so EF
    // re-parameterizes it per context instance:
    //   e => !EF.Property<bool>(e,"IsDeleted") && EF.Property<Guid?>(e,"ClinicId") == _currentClinicId
}
```

The dynamic `BuildClinicScopedFilter` must emit a **`this`-rooted member access**
to the resolved id (so EF rebinds it to the executing context), instead of
`Expression.Constant(_currentClinicService)` which freezes the instance.

### Regression test (required)

Add a test that reuses **one** model/service-provider across two contexts scoped to
**different** clinics and asserts each sees only its own rows. This reproduces the
production reuse the current tests skip. Suggested shape:

- Build one `IServiceProvider` / model cache.
- Create context A (clinic 1) → assert only clinic-1 rows.
- Create context B (clinic 2) from the **same** provider/model → assert only clinic-2 rows.
- Fails on current code, passes after the fix.

## Immediate mitigation (until fixed)

- Treat clinic isolation in the admin panel as **not enforced**. Avoid onboarding a
  second real clinic into the shared production database until patched.
- The Bethlehem account itself is correctly configured — no SQL change needed there;
  the fix is entirely in application code.

## Files referenced

- `Backend/PhysioLink.Infrastructure/Data/PhysioLinkDbContext.cs` — filter (bug site)
- `Backend/PhysioLink.API/Program.cs` — startup migration builds/caches the model
- `Backend/PhysioLink.Infrastructure/Services/CurrentClinicService.cs` — reads `ClinicId` claim
- `Backend/PhysioLink.Infrastructure/Services/TokenService.cs` — puts `ClinicId` in the JWT
- `Backend/PhysioLink.Tests/ClinicIsolationTests.cs` — tests that miss the bug
