namespace PhysioLink.Application.DTOs.Slots
{
    // An available slot offered to the admin when manually booking an appointment
    // (the "New Appointment" modal). Only Available, future slots are returned.
    public class TherapistSlotOptionDto
    {
        public Guid SlotId { get; set; }
        public DateTime ScheduledAt { get; set; }  // UTC start of the hour block
    }
}
