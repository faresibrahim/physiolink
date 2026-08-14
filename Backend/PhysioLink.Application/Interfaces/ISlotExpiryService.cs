namespace PhysioLink.Application.Interfaces
{
    // Lazy sweep (spec D7 / 3.3): no background job. Called at the start of every
    // read path that cares about appointment state — the patient slot list, the
    // patient appointment list, the admin requests queue, and the admin board /
    // History views.
    public interface ISlotExpiryService
    {
        // Materializes two time-driven transitions on the current clinic's rows:
        //   • expired Requested appointments (Status -> Expired), freeing their
        //     still-Requested slots (Status -> Available); and
        //   • Confirmed appointments whose due date has passed (Status -> Completed),
        //     so they move from the active board into History.
        // Returns how many appointments expired.
        Task<int> SweepAsync(CancellationToken ct = default);
    }
}
