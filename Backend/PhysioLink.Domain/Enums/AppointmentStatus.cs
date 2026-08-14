namespace PhysioLink.Domain.Enums
{
    // Expanded from the old { Pending, Confirmed, Cancelled, Completed } set so a
    // reject can be told apart from an expiry and from a clinic-initiated cancel.
    // Explicit numeric values are pinned because rows are stored as ints and the
    // AddAppointmentSlots migration remaps any legacy values (see spec 1.2 / 1.8).
    public enum AppointmentStatus
    {
        Requested = 0,         // pending therapist decision (the request time is CreatedAt)
        Confirmed = 1,         // therapist accepted
        Completed = 2,         // session happened
        Rejected = 3,          // therapist declined the request
        Expired = 4,           // 48h / slot-start passed with no decision
        CancelledByClinic = 5  // clinic cancelled a previously-confirmed appointment
    }
}
