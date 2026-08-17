using Microsoft.EntityFrameworkCore;
using PhysioLink.Domain.Entities;
using PhysioLink.Domain.Enums;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Tests;

// Regression test for the cross-tenant data leak (docs/tenancy-leak-query-filter.md).
//
// The existing ClinicIsolationTests / AppointmentSlotIsolationTests build a FRESH
// DbContextOptions per context with EnableServiceProviderCaching(false), so each context
// gets a newly-compiled model carrying the right clinic — they can never observe the bug.
//
// In production the EF model is compiled ONCE (startup migration) and cached app-wide, then
// reused by every request. This test reproduces that by sharing a SINGLE DbContextOptions
// instance across two contexts scoped to different clinics: the model is built once (by the
// clinic-1 context) and reused (by the clinic-2 context). With the old frozen-constant filter
// the clinic-2 context inherits clinic 1's scope and sees its data; with the per-request
// context-rooted filter it correctly sees only its own.
public class ClinicModelCacheIsolationTests
{
    private static readonly Guid Clinic1Id = new Guid("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Clinic2Id = new Guid("22222222-2222-2222-2222-222222222222");

    // Seeded therapist in clinic 1 (DbSeeder C1T1Id).
    private static readonly Guid Clinic1TherapistId = new Guid("11111111-1111-1111-1111-100000000001");

    private const string ConnectionString =
        "Host=localhost;Port=5433;Database=PhysioLinkDb;Username=postgres;Password=Ibra@2026";

    // ONE options instance, deliberately WITHOUT EnableServiceProviderCaching(false), reused
    // for both clinics so the compiled model is shared — the cached-model path where the leak lives.
    private static DbContextOptions<PhysioLinkDbContext> BuildSharedOptions() =>
        new DbContextOptionsBuilder<PhysioLinkDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

    private static PhysioLinkDbContext CreateContext(
        DbContextOptions<PhysioLinkDbContext> options, Guid clinicId)
        => new PhysioLinkDbContext(options, new FakeClinicService(clinicId));

    [Fact]
    public async Task Clinic2_Reusing_Cached_Model_Cannot_See_Clinic1_Data()
    {
        var options = BuildSharedOptions();
        // Distinctive future hour so the (TherapistId, ScheduledAt) unique index never clashes.
        var scheduledAt = new DateTime(2099, 1, 2, 9, 0, 0, DateTimeKind.Utc);

        Guid slotId;
        // Clinic 1 context is created FIRST — it triggers model compilation and caching.
        await using (var clinic1 = CreateContext(options, Clinic1Id))
        {
            await CleanupAsync(clinic1, scheduledAt);

            var slot = new AppointmentSlot
            {
                ClinicId = Clinic1Id,
                TherapistId = Clinic1TherapistId,
                ScheduledAt = scheduledAt,
                Status = SlotStatus.Available
            };
            clinic1.AppointmentSlots.Add(slot);
            await clinic1.SaveChangesAsync();
            slotId = slot.Id;
        }

        try
        {
            // Clinic 2 reuses the SAME options -> SAME cached model. It must still be scoped
            // to clinic 2 and therefore must NOT see clinic 1's slot. This assertion fails on
            // the frozen-constant bug and passes once the filter resolves the clinic per request.
            await using var clinic2 = CreateContext(options, Clinic2Id);
            var visibleToClinic2 = await clinic2.AppointmentSlots.AnyAsync(s => s.Id == slotId);
            Assert.False(visibleToClinic2,
                "Clinic 2 reusing the cached model must not see clinic 1's slot (tenancy leak regression).");

            // Sanity: the clinic-1 scope, on the same shared model, still sees its own row.
            await using var clinic1Again = CreateContext(options, Clinic1Id);
            var visibleToClinic1 = await clinic1Again.AppointmentSlots.AnyAsync(s => s.Id == slotId);
            Assert.True(visibleToClinic1, "Clinic 1 must still see its own slot on the shared model.");
        }
        finally
        {
            await using var cleanup = CreateContext(options, Clinic1Id);
            await CleanupAsync(cleanup, scheduledAt);
        }
    }

    private static async Task CleanupAsync(PhysioLinkDbContext context, DateTime scheduledAt)
    {
        var leftovers = await context.AppointmentSlots
            .IgnoreQueryFilters()
            .Where(s => s.TherapistId == Clinic1TherapistId && s.ScheduledAt == scheduledAt)
            .ToListAsync();
        if (leftovers.Count > 0)
        {
            context.AppointmentSlots.RemoveRange(leftovers);
            await context.SaveChangesAsync();
        }
    }
}
