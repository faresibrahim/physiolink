namespace PhysioLink.Application.DTOs.Slots
{
    // Body for toggling a cell on (spec 2.2). The therapist comes from the route.
    public class CreateSlotDto
    {
        public DateTime ScheduledAt { get; set; }
    }
}
