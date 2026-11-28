using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PhysioLink.Application.DTOs.Appointments;
using PhysioLink.Application.DTOs.Slots;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Entities;
using PhysioLink.Domain.Enums;
using PhysioLink.Infrastructure.Data;
using PhysioLink.Infrastructure.Services;

namespace PhysioLink.Tests;

// Patient rescheduling (POST /patients/me/appointments/{id}/reschedule) and how the
// clinic's accept / reject settle it. Runs against the local dev Postgres like the
// other DB tests (needs TEST_DB_PASSWORD and the AddAppointmentRescheduleLink
// migration applied). Test rows live in 2099 on seeded therapist C1T1 and are
// removed afterwards.
public class RescheduleTests : IAsyncLifetime
{
    private static readonly Guid Clinic1Id = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TherapistId = new("11111111-1111-1111-1111-100000000001");
    private const string PatientUsername = "alice.cooper"; // seeded, assigned to C1T1

    private static readonly DateTime SlotA = new(2099, 3, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime SlotB = new(2099, 3, 2, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime SlotC = new(2099, 3, 3, 9, 0, 0, DateTimeKind.Utc);

    private Guid _userId;
    private Guid _patientId;
    private DateTime? _soonTime; // the "inside the cutoff" appointment, if a test made one

    private static PhysioLinkDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PhysioLinkDbContext>()
            .UseNpgsql(TestDb.ConnectionString)
            .EnableServiceProviderCaching(false)
            .Options;
        return new PhysioLinkDbContext(options, new FakeClinicService(Clinic1Id));
    }

    private static PatientSlotService PatientService(PhysioLinkDbContext db) =>
        new(db, new SlotExpiryService(db));

    private static AdminAppointmentService AdminService(PhysioLinkDbContext db) =>
        new(db, new FakeClinicService(Clinic1Id), new SlotExpiryService(db),
            new NoopPushSender(), NullLogger<AdminAppointmentService>.Instance);

    public async Task InitializeAsync()
    {
        await using var db = CreateContext();
        var patient = await db.Patients.AsNoTracking().FirstAsync(p => p.Username == PatientUsername);
        _userId = patient.ApplicationUserId;
        _patientId = patient.PatientId;
        await CleanupAsync(db);
    }

    public async Task DisposeAsync()
    {
        await using var db = CreateContext();
        await CleanupAsync(db);
    }

    [Fact]
    public async Task Confirmed_Appointment_Gets_A_Linked_Request_And_Stays_Booked()
    {
        var (original, _) = await SeedAppointmentAsync(SlotA, AppointmentStatus.Confirmed, SlotStatus.Booked);
        var slotB = await SeedSlotAsync(SlotB);

        await using var db = CreateContext();
        var result = await PatientService(db).RescheduleAsync(_userId, original, new RescheduleAppointmentDto { SlotId = slotB });

        Assert.Equal(RescheduleOutcome.Requested, result.Outcome);
        Assert.Equal(original, result.Appointment!.RescheduleOfAppointmentId);
        Assert.Equal("Requested", result.Appointment.Status);

        await using var check = CreateContext();
        Assert.Equal(AppointmentStatus.Confirmed, (await check.Appointments.FindAsync(original))!.Status);
        Assert.Equal(SlotStatus.Requested, (await check.AppointmentSlots.FindAsync(slotB))!.Status);
    }

    [Fact]
    public async Task Second_Reschedule_While_One_Is_Pending_Is_Refused()
    {
        var (original, _) = await SeedAppointmentAsync(SlotA, AppointmentStatus.Confirmed, SlotStatus.Booked);
        var slotB = await SeedSlotAsync(SlotB);
        var slotC = await SeedSlotAsync(SlotC);

        await using (var db = CreateContext())
            await PatientService(db).RescheduleAsync(_userId, original, new RescheduleAppointmentDto { SlotId = slotB });

        await using var again = CreateContext();
        var result = await PatientService(again).RescheduleAsync(_userId, original, new RescheduleAppointmentDto { SlotId = slotC });

        Assert.Equal(RescheduleOutcome.NotReschedulable, result.Outcome);
    }

    [Fact]
    public async Task Pending_Request_Is_Moved_In_Place_And_Frees_Its_Old_Slot()
    {
        var (request, slotA) = await SeedAppointmentAsync(SlotA, AppointmentStatus.Requested, SlotStatus.Requested);
        var slotB = await SeedSlotAsync(SlotB);

        await using var db = CreateContext();
        var result = await PatientService(db).RescheduleAsync(_userId, request, new RescheduleAppointmentDto { SlotId = slotB });

        Assert.Equal(RescheduleOutcome.Moved, result.Outcome);
        Assert.Equal(request, result.Appointment!.AppointmentId);
        Assert.Equal(SlotB, result.Appointment.ScheduledAt);

        await using var check = CreateContext();
        Assert.Equal(SlotStatus.Available, (await check.AppointmentSlots.FindAsync(slotA))!.Status);
        Assert.Equal(SlotStatus.Requested, (await check.AppointmentSlots.FindAsync(slotB))!.Status);
    }

    [Fact]
    public async Task Inside_The_Cutoff_Is_Refused()
    {
        // Minute precision keeps cleanup able to find it again.
        var soon = DateTime.UtcNow.AddHours(6);
        _soonTime = new DateTime(soon.Year, soon.Month, soon.Day, soon.Hour, soon.Minute, 0, DateTimeKind.Utc);
        var (original, _) = await SeedAppointmentAsync(_soonTime.Value, AppointmentStatus.Confirmed, slotStatus: null);
        var slotB = await SeedSlotAsync(SlotB);

        await using var db = CreateContext();
        var result = await PatientService(db).RescheduleAsync(_userId, original, new RescheduleAppointmentDto { SlotId = slotB });

        Assert.Equal(RescheduleOutcome.TooLate, result.Outcome);
    }

    [Fact]
    public async Task Taken_Slot_Is_Refused()
    {
        var (original, _) = await SeedAppointmentAsync(SlotA, AppointmentStatus.Confirmed, SlotStatus.Booked);
        var slotB = await SeedSlotAsync(SlotB, SlotStatus.Booked);

        await using var db = CreateContext();
        var result = await PatientService(db).RescheduleAsync(_userId, original, new RescheduleAppointmentDto { SlotId = slotB });

        Assert.Equal(RescheduleOutcome.SlotUnavailable, result.Outcome);
    }

    [Fact]
    public async Task Another_Patients_Appointment_Reads_As_Not_Found()
    {
        var (original, _) = await SeedAppointmentAsync(SlotA, AppointmentStatus.Confirmed, SlotStatus.Booked);
        var slotB = await SeedSlotAsync(SlotB);

        await using var db = CreateContext();
        var result = await PatientService(db).RescheduleAsync(Guid.NewGuid(), original, new RescheduleAppointmentDto { SlotId = slotB });

        Assert.Equal(RescheduleOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task Accepting_The_Request_Retires_The_Original_And_Frees_Its_Slot()
    {
        var (original, slotA) = await SeedAppointmentAsync(SlotA, AppointmentStatus.Confirmed, SlotStatus.Booked);
        var slotB = await SeedSlotAsync(SlotB);

        Guid requestId;
        await using (var db = CreateContext())
            requestId = (await PatientService(db).RescheduleAsync(_userId, original, new RescheduleAppointmentDto { SlotId = slotB }))
                .Appointment!.AppointmentId;

        await using (var admin = CreateContext())
            Assert.Equal(AppointmentActionOutcome.Ok, await AdminService(admin).AcceptAsync(requestId));

        await using var check = CreateContext();
        Assert.Equal(AppointmentStatus.Rescheduled, (await check.Appointments.FindAsync(original))!.Status);
        Assert.Equal(AppointmentStatus.Confirmed, (await check.Appointments.FindAsync(requestId))!.Status);
        Assert.Equal(SlotStatus.Available, (await check.AppointmentSlots.FindAsync(slotA))!.Status);
        Assert.Equal(SlotStatus.Booked, (await check.AppointmentSlots.FindAsync(slotB))!.Status);

        // The replaced appointment no longer shows in the patient's list.
        var mine = await PatientService(check).GetMyAppointmentsAsync(_userId);
        Assert.DoesNotContain(mine, a => a.AppointmentId == original);
        Assert.Contains(mine, a => a.AppointmentId == requestId);
    }

    [Fact]
    public async Task Rejecting_The_Request_Keeps_The_Original_Booked()
    {
        var (original, slotA) = await SeedAppointmentAsync(SlotA, AppointmentStatus.Confirmed, SlotStatus.Booked);
        var slotB = await SeedSlotAsync(SlotB);

        Guid requestId;
        await using (var db = CreateContext())
            requestId = (await PatientService(db).RescheduleAsync(_userId, original, new RescheduleAppointmentDto { SlotId = slotB }))
                .Appointment!.AppointmentId;

        await using (var admin = CreateContext())
            Assert.Equal(AppointmentActionOutcome.Ok, await AdminService(admin).RejectAsync(requestId));

        await using var check = CreateContext();
        Assert.Equal(AppointmentStatus.Confirmed, (await check.Appointments.FindAsync(original))!.Status);
        Assert.Equal(AppointmentStatus.Rejected, (await check.Appointments.FindAsync(requestId))!.Status);
        Assert.Equal(SlotStatus.Booked, (await check.AppointmentSlots.FindAsync(slotA))!.Status);
        Assert.Equal(SlotStatus.Available, (await check.AppointmentSlots.FindAsync(slotB))!.Status);
    }

    // ── helpers ─────────────────────────────────────────────────────────────────

    private async Task<Guid> SeedSlotAsync(DateTime at, SlotStatus status = SlotStatus.Available)
    {
        await using var db = CreateContext();
        var slot = new AppointmentSlot { ClinicId = Clinic1Id, TherapistId = TherapistId, ScheduledAt = at, Status = status };
        db.AppointmentSlots.Add(slot);
        await db.SaveChangesAsync();
        return slot.Id;
    }

    private async Task<(Guid AppointmentId, Guid? SlotId)> SeedAppointmentAsync(
        DateTime at, AppointmentStatus status, SlotStatus? slotStatus)
    {
        Guid? slotId = slotStatus is { } s ? await SeedSlotAsync(at, s) : null;

        await using var db = CreateContext();
        var appointment = new Appointment
        {
            PatientId = _patientId,
            ClinicId = Clinic1Id,
            TherapistId = TherapistId,
            TherapistName = "Test Therapist",
            AppointmentSlotId = slotId,
            AppointmentTime = at,
            Title = "Follow-up",
            Status = status
        };
        db.Appointments.Add(appointment);
        await db.SaveChangesAsync();
        return (appointment.AppointmentId, slotId);
    }

    private async Task CleanupAsync(PhysioLinkDbContext db)
    {
        var times = new List<DateTime> { SlotA, SlotB, SlotC };
        if (_soonTime is { } soon) times.Add(soon);

        var appointments = await db.Appointments.IgnoreQueryFilters()
            .Where(a => a.TherapistId == TherapistId && times.Contains(a.AppointmentTime))
            .ToListAsync();
        // Break the self-reference before deleting so row order doesn't matter.
        foreach (var a in appointments) a.RescheduleOfAppointmentId = null;
        await db.SaveChangesAsync();
        db.Appointments.RemoveRange(appointments);
        await db.SaveChangesAsync();

        var slots = await db.AppointmentSlots.IgnoreQueryFilters()
            .Where(s => s.TherapistId == TherapistId && times.Contains(s.ScheduledAt))
            .ToListAsync();
        db.AppointmentSlots.RemoveRange(slots);
        await db.SaveChangesAsync();
    }

    private sealed class NoopPushSender : IPushNotificationSender
    {
        public Task SendAsync(string deviceToken, string title, string body,
            IDictionary<string, string> data, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
