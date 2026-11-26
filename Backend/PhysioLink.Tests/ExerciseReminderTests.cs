using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using PhysioLink.API.Controllers;
using PhysioLink.Application.Common;
using PhysioLink.Application.DTOs.Patients;
using PhysioLink.Application.Services;
using PhysioLink.Application.Validators.Patients;
using PhysioLink.Domain.Entities;
using PhysioLink.Domain.Enums;
using PhysioLink.Infrastructure.Data;
using PhysioLink.Infrastructure.Repositories;
using PhysioLink.Infrastructure.Services;

namespace PhysioLink.Tests;

// Daily exercise reminder: GET/PUT patients/me/reminder and the once-a-minute
// ReminderDispatcher. The DB tests run against the local dev Postgres (see TestDb)
// inside a transaction that is always rolled back, so nothing they seed — or any
// LastReminderSentOn the dispatcher stamps on other rows — survives the test.
public class ExerciseReminderTests
{
    // A plain winter weekday in the clinic zone (well clear of the DST switches).
    private static DateTime ClinicUtc(int hour, int minute, int day = 10) =>
        ClinicTime.ToUtc(new DateTime(2026, 3, day, hour, minute, 30));

    private static readonly DateOnly Day = new(2026, 3, 10);

    // ---- Validation -------------------------------------------------------------

    [Theory]
    [InlineData("25:99")]
    [InlineData("24:00")]
    [InlineData("7:30")]
    [InlineData("18:00:00")]
    [InlineData("18:5")]
    [InlineData("6pm")]
    [InlineData("")]
    public void Put_Rejects_A_Time_That_Is_Not_HHmm(string time)
    {
        var result = new UpdateReminderValidator().Validate(new UpdateReminderDto { Enabled = true, Time = time });

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("00:00")]
    [InlineData("07:30")]
    [InlineData("23:59")]
    public void Put_Accepts_A_Valid_HHmm_Time(string time)
    {
        var result = new UpdateReminderValidator().Validate(new UpdateReminderDto { Enabled = true, Time = time });

        Assert.True(result.IsValid);
    }

    // ---- Endpoints act on the JWT caller only -------------------------------------

    [Fact]
    public async Task Get_And_Put_Use_The_Patient_From_The_Jwt()
    {
        var callerPatientId = Guid.NewGuid();
        var service = new FakePatientService(callerPatientId);
        var controller = new PatientController(service) { ControllerContext = ContextForAnyCaller() };

        Assert.IsType<OkObjectResult>(await controller.GetReminder());
        Assert.Equal(callerPatientId, service.LastReminderPatientId);

        Assert.IsType<OkObjectResult>(
            await controller.UpdateReminder(new UpdateReminderDto { Enabled = true, Time = "07:30" }));
        Assert.Equal(callerPatientId, service.LastReminderPatientId);
    }

    [Fact]
    public async Task Get_Is_Forbidden_For_A_User_With_No_Patient_Record()
    {
        var controller = new PatientController(new FakePatientService(null)) { ControllerContext = ContextForAnyCaller() };

        Assert.IsType<ForbidResult>(await controller.GetReminder());
    }

    // ---- Settings persistence ---------------------------------------------------

    [Fact]
    public async Task Get_Returns_Defaults_For_A_New_Patient()
    {
        await using var db = await ReminderDb.OpenAsync();
        var patient = await db.AddPatientAsync(optedIn: false);   // reminder fields never set

        var reminder = await db.Service(ClinicUtc(12, 0)).GetReminderAsync(patient.PatientId);

        Assert.NotNull(reminder);
        Assert.False(reminder.Enabled);
        Assert.Equal("18:00", reminder.Time);
    }

    [Fact]
    public async Task Put_Persists_And_Get_Returns_It()
    {
        await using var db = await ReminderDb.OpenAsync();
        var patient = await db.AddPatientAsync(optedIn: false);
        var service = db.Service(ClinicUtc(6, 0));

        var saved = await service.UpdateReminderAsync(patient.PatientId, new UpdateReminderDto { Enabled = true, Time = "07:30" });
        var reminder = await service.GetReminderAsync(patient.PatientId);

        Assert.Equal("07:30", saved!.Time);
        Assert.True(reminder!.Enabled);
        Assert.Equal("07:30", reminder.Time);
        // 07:30 is still ahead at 06:00, so today's reminder is still pending.
        Assert.Null(await db.LastSentOnAsync(patient.PatientId));
    }

    [Fact]
    public async Task Enabling_After_Todays_Time_Has_Passed_Does_Not_Fire_A_Stale_Reminder()
    {
        await using var db = await ReminderDb.OpenAsync();
        var patient = await db.AddPatientAsync(configure: p => p.ReminderEnabled = false);

        // Patient turns the 18:00 reminder on at 18:30 — within the 2h send window,
        // but the reminder must start tomorrow, not fire on the next tick.
        await db.Service(ClinicUtc(18, 30))
            .UpdateReminderAsync(patient.PatientId, new UpdateReminderDto { Enabled = true, Time = "18:00" });
        await db.Dispatcher.RunOnceAsync(ClinicUtc(18, 31));

        Assert.Empty(db.Push.SendsTo(patient.DeviceToken!));
        Assert.Equal(Day, await db.LastSentOnAsync(patient.PatientId));

        await db.Dispatcher.RunOnceAsync(ClinicUtc(18, 0, day: 11));
        Assert.Single(db.Push.SendsTo(patient.DeviceToken!));
    }

    // ---- Dispatcher -------------------------------------------------------------

    [Fact]
    public async Task Sends_Once_At_The_Clinic_Local_Time_And_Not_Again_On_The_Next_Tick()
    {
        await using var db = await ReminderDb.OpenAsync();
        var patient = await db.AddPatientAsync();

        await db.Dispatcher.RunOnceAsync(ClinicUtc(18, 0));
        await db.Dispatcher.RunOnceAsync(ClinicUtc(18, 1));
        await db.Dispatcher.RunOnceAsync(ClinicUtc(19, 30));

        var send = Assert.Single(db.Push.SendsTo(patient.DeviceToken!));
        Assert.Equal("reminder", send.Data["type"]);
        Assert.Equal(ReminderMessages.NotStartedTitle, send.Title);
        Assert.False(string.IsNullOrWhiteSpace(send.Body));
        Assert.Equal(Day, await db.LastSentOnAsync(patient.PatientId));
    }

    [Fact]
    public async Task Does_Not_Send_Before_The_Time()
    {
        await using var db = await ReminderDb.OpenAsync();
        var patient = await db.AddPatientAsync();

        await db.Dispatcher.RunOnceAsync(ClinicUtc(17, 59));

        Assert.Empty(db.Push.SendsTo(patient.DeviceToken!));
        Assert.Null(await db.LastSentOnAsync(patient.PatientId));
    }

    [Fact]
    public async Task Does_Not_Send_When_Disabled()
    {
        await using var db = await ReminderDb.OpenAsync();
        var patient = await db.AddPatientAsync(configure: p => p.ReminderEnabled = false);

        await db.Dispatcher.RunOnceAsync(ClinicUtc(18, 5));

        Assert.Empty(db.Push.SendsTo(patient.DeviceToken!));
        Assert.Null(await db.LastSentOnAsync(patient.PatientId));
    }

    [Fact]
    public async Task Does_Not_Send_Without_A_Device_Token()
    {
        await using var db = await ReminderDb.OpenAsync();
        var patient = await db.AddPatientAsync(configure: p => p.DeviceToken = null);

        await db.Dispatcher.RunOnceAsync(ClinicUtc(18, 5));

        // Not even claimed: once a token is registered later (within the window)
        // the reminder can still go out today.
        Assert.Null(await db.LastSentOnAsync(patient.PatientId));
    }

    [Fact]
    public async Task Skips_But_Marks_Handled_When_There_Are_No_Active_Assignments()
    {
        await using var db = await ReminderDb.OpenAsync();
        var patient = await db.AddPatientAsync(assignments: new[] { AssignmentStatus.Paused, AssignmentStatus.Completed });

        await db.Dispatcher.RunOnceAsync(ClinicUtc(18, 5));

        Assert.Empty(db.Push.SendsTo(patient.DeviceToken!));
        Assert.Equal(Day, await db.LastSentOnAsync(patient.PatientId));
    }

    [Fact]
    public async Task Skips_When_Every_Active_Exercise_Was_Logged_Today()
    {
        await using var db = await ReminderDb.OpenAsync();
        var patient = await db.AddPatientAsync(assignments: new[] { AssignmentStatus.Active, AssignmentStatus.Active });
        await db.CompleteAssignmentsAsync(patient.PatientId, count: 2, at: ClinicUtc(9, 0));

        await db.Dispatcher.RunOnceAsync(ClinicUtc(18, 5));

        Assert.Empty(db.Push.SendsTo(patient.DeviceToken!));
        Assert.Equal(Day, await db.LastSentOnAsync(patient.PatientId));
    }

    [Fact]
    public async Task Sends_Almost_There_When_Some_Exercises_Were_Logged_Today()
    {
        await using var db = await ReminderDb.OpenAsync();
        var patient = await db.AddPatientAsync(assignments: new[] { AssignmentStatus.Active, AssignmentStatus.Active });
        await db.CompleteAssignmentsAsync(patient.PatientId, count: 1, at: ClinicUtc(9, 0));

        await db.Dispatcher.RunOnceAsync(ClinicUtc(18, 5));

        var send = Assert.Single(db.Push.SendsTo(patient.DeviceToken!));
        Assert.Equal(ReminderMessages.PartlyDoneTitle, send.Title);
    }

    [Fact]
    public async Task Exercises_Logged_Yesterday_Do_Not_Count_As_Done_Today()
    {
        await using var db = await ReminderDb.OpenAsync();
        var patient = await db.AddPatientAsync();
        // 23:30 clinic time the previous evening — after midnight UTC-wise it's
        // still "yesterday" for the clinic.
        await db.CompleteAssignmentsAsync(patient.PatientId, count: 1, at: ClinicUtc(23, 30, day: 9));

        await db.Dispatcher.RunOnceAsync(ClinicUtc(18, 5));

        var send = Assert.Single(db.Push.SendsTo(patient.DeviceToken!));
        Assert.Equal(ReminderMessages.NotStartedTitle, send.Title);
    }

    [Fact]
    public async Task A_Reminder_More_Than_Two_Hours_Late_Is_Skipped_But_Marked_Handled()
    {
        await using var db = await ReminderDb.OpenAsync();
        var patient = await db.AddPatientAsync();

        // e.g. the API was down from 17:50 to 20:01.
        var summary = await db.Dispatcher.RunOnceAsync(ClinicUtc(20, 1));

        Assert.Empty(db.Push.SendsTo(patient.DeviceToken!));
        Assert.Equal(Day, await db.LastSentOnAsync(patient.PatientId));
        Assert.True(summary.Stale >= 1);
    }

    [Fact]
    public async Task A_Reminder_Less_Than_Two_Hours_Late_Is_Still_Sent()
    {
        await using var db = await ReminderDb.OpenAsync();
        var patient = await db.AddPatientAsync();

        await db.Dispatcher.RunOnceAsync(ClinicUtc(19, 59));

        Assert.Single(db.Push.SendsTo(patient.DeviceToken!));
    }

    [Fact]
    public async Task Sends_Again_On_The_Next_Clinic_Day()
    {
        await using var db = await ReminderDb.OpenAsync();
        var patient = await db.AddPatientAsync();

        await db.Dispatcher.RunOnceAsync(ClinicUtc(18, 0));
        await db.Dispatcher.RunOnceAsync(ClinicUtc(18, 0, day: 11));

        Assert.Equal(2, db.Push.SendsTo(patient.DeviceToken!).Count);
    }

    [Fact]
    public async Task Inactive_And_Soft_Deleted_Patients_Are_Excluded()
    {
        await using var db = await ReminderDb.OpenAsync();
        var inactive = await db.AddPatientAsync(configure: p => p.IsActive = false);
        var deleted = await db.AddPatientAsync(configure: p => p.IsDeleted = true);

        await db.Dispatcher.RunOnceAsync(ClinicUtc(18, 5));

        Assert.Empty(db.Push.SendsTo(inactive.DeviceToken!));
        Assert.Empty(db.Push.SendsTo(deleted.DeviceToken!));
    }

    [Fact]
    public async Task A_Failed_Send_Does_Not_Stop_The_Others_Or_Retry()
    {
        await using var db = await ReminderDb.OpenAsync();
        var failing = await db.AddPatientAsync();
        var healthy = await db.AddPatientAsync();
        db.Push.FailFor(failing.DeviceToken!);

        await db.Dispatcher.RunOnceAsync(ClinicUtc(18, 0));
        await db.Dispatcher.RunOnceAsync(ClinicUtc(18, 1));

        Assert.Single(db.Push.SendsTo(healthy.DeviceToken!));
        // Attempted once, then the day stays claimed: at-most-once, no retry storm.
        Assert.Single(db.Push.SendsTo(failing.DeviceToken!));
        Assert.Equal(Day, await db.LastSentOnAsync(failing.PatientId));
    }

    // ---- Helpers ----------------------------------------------------------------

    private static ControllerContext ContextForAnyCaller()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
        }, "TestAuth");
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };
    }

    // One DbContext + an open transaction that is rolled back on dispose.
    private sealed class ReminderDb : IAsyncDisposable
    {
        private readonly IDbContextTransaction _transaction;

        public PhysioLinkDbContext Context { get; }
        public FakePushSender Push { get; } = new();
        public ReminderDispatcher Dispatcher { get; }

        private ReminderDb(PhysioLinkDbContext context, IDbContextTransaction transaction)
        {
            Context = context;
            _transaction = transaction;
            Dispatcher = new ReminderDispatcher(context, Push, NullLogger<ReminderDispatcher>.Instance);
        }

        public static async Task<ReminderDb> OpenAsync()
        {
            var options = new DbContextOptionsBuilder<PhysioLinkDbContext>()
                .UseNpgsql(TestDb.ConnectionString)
                .EnableServiceProviderCaching(false)
                .Options;
            var context = new PhysioLinkDbContext(options, new FakeClinicService(Guid.Empty));
            var transaction = await context.Database.BeginTransactionAsync();
            return new ReminderDb(context, transaction);
        }

        public PatientService Service(DateTime utcNow) =>
            new(new PatientRepository(Context), new FixedTimeProvider(utcNow));

        // Defaults to: reminder on at 18:00, a device token, one active assignment.
        // optedIn: false leaves the reminder fields exactly as a new Patient has them.
        public async Task<Patient> AddPatientAsync(
            Action<Patient>? configure = null,
            AssignmentStatus[]? assignments = null,
            bool optedIn = true)
        {
            var clinicId = await Context.Clinics.IgnoreQueryFilters().Select(c => c.ClinicId).FirstAsync();
            var exerciseId = await Context.Exercises.IgnoreQueryFilters().Select(e => e.ExerciseId).FirstAsync();
            var suffix = Guid.NewGuid().ToString("N");

            var user = new ApplicationUser("Reminder", "Test", null, "not-a-real-hash", "Patient", clinicId)
            {
                Username = $"rem-{suffix}",
            };
            var patient = new Patient("Reminder", "Test", "0000000000", user.ApplicationUserId, $"rem-{suffix}", null, "test")
            {
                ClinicId = clinicId,
                DeviceToken = $"test-reminder-token-{suffix}",
            };
            if (optedIn)
            {
                patient.ReminderEnabled = true;
                patient.ReminderTime = new TimeOnly(18, 0);
            }
            configure?.Invoke(patient);

            Context.Users.Add(user);
            Context.Patients.Add(patient);
            foreach (var status in assignments ?? new[] { AssignmentStatus.Active })
            {
                Context.ExerciseAssignments.Add(
                    new ExerciseAssignment("Test", patient.PatientId, exerciseId, 3, 10, 5, 3, null, DateTime.UtcNow)
                    {
                        Status = status,
                    });
            }

            await Context.SaveChangesAsync();
            return patient;
        }

        public async Task CompleteAssignmentsAsync(Guid patientId, int count, DateTime at)
        {
            var toComplete = await Context.ExerciseAssignments
                .Where(a => a.PatientId == patientId && a.Status == AssignmentStatus.Active)
                .Take(count)
                .ToListAsync();
            foreach (var assignment in toComplete)
            {
                assignment.Feedback = 3;
                assignment.CompletedAt = at;
            }
            await Context.SaveChangesAsync();
        }

        public Task<DateOnly?> LastSentOnAsync(Guid patientId) =>
            Context.Patients.IgnoreQueryFilters().AsNoTracking()
                .Where(p => p.PatientId == patientId)
                .Select(p => p.LastReminderSentOn)
                .FirstAsync();

        public async ValueTask DisposeAsync()
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            await Context.DisposeAsync();
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;
        public FixedTimeProvider(DateTime utcNow) => _utcNow = new DateTimeOffset(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc));
        public override DateTimeOffset GetUtcNow() => _utcNow;
    }

    internal sealed record SentPush(string Token, string Title, string Body, IDictionary<string, string> Data);

    // Hand-written IPushNotificationSender fake (no mocking library in this
    // project). Records every attempt; tokens passed to FailFor throw.
    internal sealed class FakePushSender : IPushNotificationSender
    {
        private readonly ConcurrentQueue<SentPush> _sends = new();
        private readonly ConcurrentDictionary<string, bool> _failing = new();

        public void FailFor(string token) => _failing[token] = true;

        public IReadOnlyList<SentPush> SendsTo(string token) =>
            _sends.Where(s => s.Token == token).ToList();

        public Task SendAsync(string deviceToken, string title, string body, IDictionary<string, string> data, CancellationToken cancellationToken = default)
        {
            _sends.Enqueue(new SentPush(deviceToken, title, body, data));
            if (_failing.ContainsKey(deviceToken))
                throw new InvalidOperationException("Simulated FCM failure");
            return Task.CompletedTask;
        }
    }
}
