namespace PhysioLink.Domain.Enums
{
    // A slot's lifecycle is deliberately kept separate from an appointment's
    // lifecycle (AppointmentStatus). See spec D5/D6.
    public enum SlotStatus
    {
        Available = 0, // open, bookable
        Requested = 1, // a patient has requested it; not yet decided
        Booked = 2     // therapist accepted; taken
    }
}
