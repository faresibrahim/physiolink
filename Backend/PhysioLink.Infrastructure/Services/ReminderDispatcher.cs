using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhysioLink.Application.Common;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Entities;
using PhysioLink.Domain.Enums;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Services
{
    // Sends each opted-in patient at most one exercise reminder per clinic day.
    //
    // Everything is compared on the CLINIC-local wall clock (ClinicTime): the stored
    // ReminderTime is a clinic time-of-day, so "due" is ReminderTime <= clinic now.
    // No UTC arithmetic on the stored time, so DST can't shift it; a time inside the
    // spring-forward gap (doesn't exist that day) simply becomes due once the clock
    // jumps past it, and the repeated hour at fall-back can't double-send because of
    // LastReminderSentOn.
    //
    // "<= now" + LastReminderSentOn != today means a missed tick, restart or deploy
    // still sends (late), and nothing sends twice. A reminder more than MaxLateness
    // overdue is marked handled without sending — no 18:00 nudge at 23:00.
    //
    // At-most-once: a patient is claimed (LastReminderSentOn = today, via an atomic
    // conditional UPDATE) BEFORE deciding or sending. A crash after the claim loses
    // that day's reminder rather than duplicating it, and if the API ever runs more
    // than one instance only the instance whose UPDATE hit the row sends.
    public class ReminderDispatcher : IReminderDispatcher
    {
        internal const int BatchSize = 50;
        internal static readonly TimeSpan MaxLateness = TimeSpan.FromHours(2);

        // Routes to the Exercises tab in the app. Not "exercise"/"appointment".
        internal const string PushType = "reminder";

        private readonly PhysioLinkDbContext _dbContext;
        private readonly IPushNotificationSender _pushSender;
        private readonly ILogger<ReminderDispatcher> _logger;

        public ReminderDispatcher(
            PhysioLinkDbContext dbContext,
            IPushNotificationSender pushSender,
            ILogger<ReminderDispatcher> logger)
        {
            _dbContext = dbContext;
            _pushSender = pushSender;
            _logger = logger;
        }

        public async Task<ReminderRunSummary> RunOnceAsync(DateTime utcNow, CancellationToken cancellationToken = default)
        {
            var clinicNow = ClinicTime.ToLocal(utcNow);
            var today = DateOnly.FromDateTime(clinicNow);
            var nowTime = TimeOnly.FromDateTime(clinicNow);
            var todayStartUtc = ClinicTime.ToUtc(clinicNow.Date);

            // 1. Overdue by more than MaxLateness (API was down, patient had no token
            //    until late, ...): mark today handled, don't send. Before 02:00 nothing
            //    today can be that late — yesterday's leftovers are a new day's problem.
            var stale = 0;
            if (nowTime.ToTimeSpan() >= MaxLateness)
            {
                var oldestSendable = nowTime.Add(-MaxLateness);
                stale = await DueToday(today, nowTime)
                    .Where(p => p.ReminderTime < oldestSendable)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.LastReminderSentOn, today), cancellationToken);
            }

            // 2. Everyone else who is due.
            var due = await DueToday(today, nowTime)
                .AsNoTracking()
                .OrderBy(p => p.ReminderTime)
                .Select(p => new { p.PatientId, p.DeviceToken })
                .ToListAsync(cancellationToken);

            int sent = 0, skipped = 0, failed = 0;

            foreach (var batch in due.Chunk(BatchSize))
            {
                // DB work (claim + compose) stays sequential on the one scoped
                // DbContext; the FCM sends of a batch run concurrently so one slow
                // send doesn't hold up the rest.
                var pending = new List<PendingReminder>();
                foreach (var candidate in batch)
                {
                    try
                    {
                        if (!await TryClaimAsync(candidate.PatientId, today, nowTime, cancellationToken))
                            continue; // disabled/claimed since we read it

                        var message = await ComposeAsync(candidate.PatientId, today, todayStartUtc, cancellationToken);
                        if (message is null)
                        {
                            skipped++;
                            continue;
                        }

                        pending.Add(new PendingReminder(candidate.PatientId, candidate.DeviceToken!, message));
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        failed++;
                        _logger.LogError(ex, "Exercise reminder failed for patient {PatientId}", candidate.PatientId);
                    }
                }

                var outcomes = await Task.WhenAll(pending.Select(r => SendAsync(r, cancellationToken)));

                foreach (var (reminder, outcome) in pending.Zip(outcomes))
                {
                    switch (outcome)
                    {
                        case SendOutcome.Sent:
                            sent++;
                            break;
                        case SendOutcome.Unregistered:
                            failed++;
                            await ClearDeadTokenAsync(reminder, cancellationToken);
                            break;
                        default:
                            failed++;
                            break;
                    }
                }
            }

            if (sent + skipped + stale + failed > 0)
            {
                _logger.LogInformation(
                    "Exercise reminders for {ClinicDay}: {Sent} sent, {Skipped} skipped, {Stale} stale, {Failed} failed",
                    today, sent, skipped, stale, failed);
            }

            return new ReminderRunSummary(sent, skipped, stale, failed);
        }

        // Opted-in, reachable, live patients whose clinic-local reminder time has
        // arrived and who haven't been handled today. Runs outside any HTTP request
        // (no clinic claim), so the clinic filter is bypassed and IsDeleted applied
        // by hand.
        private IQueryable<Patient> DueToday(DateOnly today, TimeOnly nowTime) =>
            _dbContext.Patients
                .IgnoreQueryFilters()
                .Where(p => !p.IsDeleted
                            && p.IsActive
                            && p.ReminderEnabled
                            && p.DeviceToken != null && p.DeviceToken != ""
                            && p.ReminderTime <= nowTime
                            && (p.LastReminderSentOn == null || p.LastReminderSentOn != today));

        // Atomic conditional claim: only one tick/instance can flip a patient's
        // LastReminderSentOn to today, and only that one goes on to send. The due
        // conditions are re-checked so a patient who disabled the reminder (or
        // logged out) a moment ago isn't claimed.
        private async Task<bool> TryClaimAsync(Guid patientId, DateOnly today, TimeOnly nowTime, CancellationToken cancellationToken)
        {
            var claimed = await DueToday(today, nowTime)
                .Where(p => p.PatientId == patientId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.LastReminderSentOn, today), cancellationToken);
            return claimed == 1;
        }

        // Null = nothing worth reminding about: no active exercises, or every active
        // exercise already logged today. Completion is ExerciseAssignment.CompletedAt
        // (set on each feedback submission), counted from clinic-local midnight.
        private async Task<ReminderMessage?> ComposeAsync(Guid patientId, DateOnly today, DateTime todayStartUtc, CancellationToken cancellationToken)
        {
            var active = _dbContext.ExerciseAssignments
                .AsNoTracking()
                .Where(a => a.PatientId == patientId && a.Status == AssignmentStatus.Active);

            var activeCount = await active.CountAsync(cancellationToken);
            if (activeCount == 0)
                return null;

            var doneToday = await active.CountAsync(a => a.CompletedAt >= todayStartUtc, cancellationToken);
            if (doneToday >= activeCount)
                return null;

            return ReminderMessages.For(doneToday, today);
        }

        // Best-effort, mirroring AdminAssignmentService.SendAssignmentPushAsync: a
        // dead token is reported back for clearing, anything else is logged and the
        // run continues.
        private async Task<SendOutcome> SendAsync(PendingReminder reminder, CancellationToken cancellationToken)
        {
            try
            {
                await _pushSender.SendAsync(
                    reminder.DeviceToken,
                    reminder.Message.Title,
                    reminder.Message.Body,
                    new Dictionary<string, string> { ["type"] = PushType },
                    cancellationToken);
                return SendOutcome.Sent;
            }
            catch (FirebaseMessagingException ex) when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                return SendOutcome.Unregistered;
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Exercise reminder push failed for patient {PatientId}", reminder.PatientId);
                return SendOutcome.Failed;
            }
        }

        // Only clears the token we actually sent to — if the patient registered a
        // new one in the meantime, keep it.
        private async Task ClearDeadTokenAsync(PendingReminder reminder, CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.Patients
                    .IgnoreQueryFilters()
                    .Where(p => p.PatientId == reminder.PatientId && p.DeviceToken == reminder.DeviceToken)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.DeviceToken, (string?)null), cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Could not clear unregistered device token for patient {PatientId}", reminder.PatientId);
            }
        }

        private enum SendOutcome { Sent, Unregistered, Failed }

        private sealed record PendingReminder(Guid PatientId, string DeviceToken, ReminderMessage Message);
    }
}
