namespace PhysioLink.Application.DTOs.Slots
{
    // Outcome of a slot create/delete so the controller can pick the HTTP status
    // without the service knowing about HTTP (mirrors the codebase's controller-maps
    // -status convention).
    public enum SlotWriteOutcome
    {
        Ok,                     // created / idempotent no-op (create) or soft-deleted (delete)
        TherapistNotFound,      // therapist doesn't exist in the caller's clinic -> 404
        NotOnTheHour,           // scheduledAt is not hour-aligned -> 400
        OutsideOperatingWindow, // hour is outside the clinic's [OpenHour, CloseHour) -> 400
        SlotNotFound,           // delete target doesn't exist -> 404
        SlotIsLive              // delete refused: slot is Requested/Booked -> 409
    }
}
