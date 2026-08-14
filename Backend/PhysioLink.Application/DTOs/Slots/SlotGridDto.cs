namespace PhysioLink.Application.DTOs.Slots
{
    // The 7-day x N-hour grid for one therapist for one week. The hour bounds come
    // from the clinic's operating window (spec 2.1). Cells are ordered day-major,
    // then hour, so the panel can lay them out row-per-hour, column-per-day.
    public class SlotGridDto
    {
        public Guid TherapistId { get; set; }
        public DateTime WeekStart { get; set; }  // UTC midnight of the first day
        public int OpenHour { get; set; }
        public int CloseHour { get; set; }
        public List<SlotCellDto> Cells { get; set; } = [];
    }
}
