namespace PhysioLink.Application.DTOs.Slots
{
    // Body for a patient requesting a slot (spec 3.2). The patient is resolved from
    // the JWT, not the body. Type maps onto the appointment's Title.
    public class RequestSlotDto
    {
        public Guid SlotId { get; set; }
        public string? Type { get; set; }
        public string? Notes { get; set; }
    }
}
