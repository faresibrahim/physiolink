# PhysioLink — Admin Panel

> ASP.NET Core MVC front end for clinic staff. Talks to the [PhysioLink API](../README.md) over HTTP — no direct database access. Deployed on Railway.

---

## Tech Stack

| Purpose | Tool |
|---|---|
| Framework | ASP.NET Core (.NET 9) MVC, Razor Views (`AddRazorRuntimeCompilation`) |
| Auth | Cookie authentication (`CookieAuthenticationDefaults`) |
| API access | `IHttpClientFactory` named client `"PhysioLinkApi"`, wrapped by `ApiClient` |
| Searchable selects | Tom Select |
| Deployment | Railway |

---

## Solution Structure

```
PhysioLink.AdminPanel/
├── Controllers/       # Dashboard, Therapists, Patients, Exercises, Assignment, Appointments, Auth, BaseController
├── Services/           # ApiClient (all API calls), response/request DTOs, SessionExpiredException
├── ViewModels/         # Per-feature view models + shared PaginationViewModel
├── Views/               # Razor views, one folder per controller
├── ViewComponents/  # RequestsNavViewComponent
├── Filters/               # SessionExpiredExceptionFilter
└── Program.cs
```

**Key architecture decisions:**

- No direct database access — every read/write goes through `ApiClient` to the API over HTTP
- `ApiClient` is registered scoped and holds tokens refreshed mid-request, since a 401-triggered refresh rotates the refresh token and `Response.Cookies` writes aren't visible back on `Request.Cookies` within the same request — without caching the refreshed pair, a page making several API calls would re-send an already-dead refresh token on every call after the first
- `BaseController.SessionExpiredRedirect()` + the global `SessionExpiredExceptionFilter` — when `ApiClient` can no longer refresh a dead session, the filter catches the resulting `SessionExpiredException` and redirects to login with a message, instead of rendering a view full of empty/zeroed data
- `ExecuteWithRefreshAsync` overloads in `ApiClient` centralize the "call → on 401 refresh once → retry" logic so individual endpoint methods stay one-liners
- Every outgoing API call carries an `X-Internal-Api-Key` header, identifying the admin panel as a trusted first-party server so its traffic bypasses the API's per-IP rate limiting (otherwise every admin's requests would share this one server's IP and get throttled together)
- Two distinct "secure cookie" mechanisms, both landing on the same production behavior: the ASP.NET Core auth cookie sets `CookieSecurePolicy.Always` outside `Development` (`SameAsRequest` in dev), while the manually-issued `auth_token`/`refresh_token` cookies (set in `AuthController` and refreshed in `ApiClient`) use the literal `Secure = !_environment.IsDevelopment()`

---

## Features

### Dashboard
Therapist/patient counts, today's appointment count, active assignment count, 5 upcoming confirmed appointments, 5 recent patients.

### Therapists
List (paged), detail (with assigned patients), create, edit, deactivate, weekly slot schedule with per-cell open/close toggling.

### Patients
List (paged, search + therapist filter), detail, create (returns a system-generated temporary password shown once), edit, deactivate (requires the admin re-entering their own account password).

### Exercises
Library browsing — search, difficulty and category filters, detail modal (description, sets/reps/duration, video). Browse-only; there is no exercise-creation flow in the UI.

### Assignments
Assign an exercise to a patient, edit an assignment's sets/reps/duration/frequency/status, unassign.

### Appointments
Week-grid and list views, create, edit, cancel, a requests queue with accept/reject, confirm/reject/cancel actions from the week view, and a history archive of completed/rejected/expired/cancelled appointments.

---

## Local Setup

### Prerequisites

- .NET 9 SDK
- The API running somewhere reachable — locally (see [`Backend/README.md`](../README.md)) or the live deployment

### Configuration

`ApiBaseUrl` is read from configuration in `Program.cs` and used as the base address for every API call. Set it in `appsettings.Development.json` or as an environment variable:

```json
{
  "ApiBaseUrl": "http://localhost:5218/"
}
```

To point at the live API instead:

```json
{
  "ApiBaseUrl": "https://physiolink-production.up.railway.app/"
}
```

### Run Locally

```bash
cd Backend/PhysioLink.AdminPanel
dotnet run
```

Listens on `http://localhost:5001` (or `https://localhost:7171;http://localhost:5001` under the `https` launch profile) — **not** the API's ports (`5218`/`7274`); this is a separate project with its own `launchSettings.json`.

### Demo Credentials

| Field | Value |
|---|---|
| Email | `fares.a.ibrahim@gmail.com` |
| Password | `test@123` |

---

## Deployment

Deployed on **Railway**, as a separate service from the API.

### Live URL

| Resource | URL |
|---|---|
| Admin Panel | `https://physiolink-production-b9b6.up.railway.app` |

### Config-as-code

Builds from [`Backend/railway.adminpanel.toml`](../railway.adminpanel.toml):

```toml
[build]
builder = "dockerfile"
dockerfilePath = "Backend/PhysioLink.AdminPanel/Dockerfile"

[deploy]
healthcheckPath = "/health"
```

`/health` is a dedicated, anonymous, no-I/O endpoint — Railway's healthcheck can't use `/`, since that's `[Authorize]` on `DashboardController` and 302-redirects unauthenticated requests to `/Auth/Login`, and a non-2xx response fails the healthcheck.

### Required environment variables

| Variable | Description |
|---|---|
| `ApiBaseUrl` | Points at the live API |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

No `CONNECTION_STRING` — this service never touches the database directly, only the API over HTTP.

---

## Screenshots

_Images go in `docs/screenshots/`._

![Dashboard](docs/screenshots/dashboard.png)
![Patients list](docs/screenshots/patients-list.png)
![Patient Detail](docs/screenshots/patient-detail.png)
![Exercises library](docs/screenshots/exercises-library.png)
![Appointments](docs/screenshots/appointments.png)

---

## License

MIT
