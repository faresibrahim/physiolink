namespace PhysioLink.Application.DTOs.Slots
{
    // One (day, hour) cell of the weekly toggle grid. State is a string the admin
    // panel maps directly to a toggle: "Empty" (off), "Available" (on/removable),
    // "Requested"/"Booked" (on/locked). SlotId is null for an Empty cell.
    public class SlotCellDto
    {
        public DateTime ScheduledAt { get; set; }   // UTC start of the hour block
        public int DayIndex { get; set; }           // 0..6 offset from the week start
        public int Hour { get; set; }               // hour of day (matches ScheduledAt)
        public string State { get; set; } = "Empty";
        public Guid? SlotId { get; set; }
    }
}
