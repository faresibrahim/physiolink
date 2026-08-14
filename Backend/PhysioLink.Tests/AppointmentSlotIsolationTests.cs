using Microsoft.EntityFrameworkCore;
using PhysioLink.Domain.Entities;
using PhysioLink.Domain.Enums;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Tests;

// Phase 1 STOP gate (spec §1.8): prove that a slot created under clinic A's context
// is invisible under clinic B's context — the global clinic query filter must cover
// the new AppointmentSlot entity too. Runs against the same local Postgres the
// existing ClinicIsolationTests use.
public class AppointmentSlotIsolationTests
{
    private static readonly Guid Clinic1Id = new Guid("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Clinic2Id = new Guid("22222222-2222-2222-2222-222222222222");

    // Seeded therapist in clinic 1 (DbSeeder C1T1Id).
    private static readonly Guid Clinic1TherapistId = new Guid("11111111-1111-1111-1111-100000000001");

    private const string ConnectionString =
        "Host=localhost;Port=5433;Database=PhysioLinkDb;Username=postgres;Password=Ibra@2026";

    private PhysioLinkDbContext CreateContext(Guid clinicId)
    {
        var options = new DbContextOptionsBuilder<PhysioLinkDbContext>()
            .UseNpgsql(ConnectionString)
            .EnableServiceProviderCaching(false)
            .Options;
        return new PhysioLinkDbContext(options, new FakeClinicService(clinicId));
    }

    [Fact]
    public async Task Slot_Created_Under_Clinic1_Is_Invisible_Under_Clinic2()
    {
        // A distinctive future hour so the (TherapistId, ScheduledAt) unique index
        // never clashes with real data.
        var scheduledAt = new DateTime(2099, 1, 1, 9, 0, 0, DateTimeKind.Utc);

        Guid slotId;
        await using (var clinic1 = CreateContext(Clinic1Id))
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
            // Clinic 2 must not see it.
            await using (var clinic2 = CreateContext(Clinic2Id))
            {
                var visibleToClinic2 = await clinic2.AppointmentSlots
                    .AnyAsync(s => s.Id == slotId);
                Assert.False(visibleToClinic2, "Clinic 2 must not see a slot created under clinic 1.");
            }

            // Clinic 1 still sees it.
            await using (var clinic1 = CreateContext(Clinic1Id))
            {
                var visibleToClinic1 = await clinic1.AppointmentSlots
                    .AnyAsync(s => s.Id == slotId);
                Assert.True(visibleToClinic1, "Clinic 1 must see its own slot.");
            }
        }
        finally
        {
            await using var cleanup = CreateContext(Clinic1Id);
            await CleanupAsync(cleanup, scheduledAt);
        }
    }

    private static async Task CleanupAsync(PhysioLinkDbContext context, DateTime scheduledAt)
    {
        // Hard-remove any test slot at this hour (ignore filters so a soft-deleted
        // leftover is also cleared) to keep the run repeatable.
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
