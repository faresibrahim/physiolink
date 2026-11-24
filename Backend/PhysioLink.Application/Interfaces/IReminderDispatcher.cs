namespace PhysioLink.Application.Interfaces
{
    // One pass of the daily exercise reminder scheduler. The hosted service calls
    // this once a minute; tests call it directly with a fixed clock.
    public interface IReminderDispatcher
    {
        Task<ReminderRunSummary> RunOnceAsync(DateTime utcNow, CancellationToken cancellationToken = default);
    }

    // Sent: push handed to FCM. Skipped: claimed for today but nothing to remind
    // about (no active exercises / all done). Stale: due more than the allowed
    // lateness ago, marked handled without sending. Failed: send threw.
    public record ReminderRunSummary(int Sent, int Skipped, int Stale, int Failed);
}
