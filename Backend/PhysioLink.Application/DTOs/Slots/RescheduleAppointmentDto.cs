namespace PhysioLink.Application.DTOs.Slots
{
    // Body for a patient moving one of their appointments to another open slot.
    public class RescheduleAppointmentDto
    {
        public Guid SlotId { get; set; }
    }
}
