using PhysioLink.AdminPanel.Services;

namespace PhysioLink.AdminPanel.ViewModels;

// Weekly toggle grid for one therapist (spec 6 "Therapist scheduling grid").
public class TherapistScheduleViewModel
{
    public Guid TherapistId { get; set; }
    public string TherapistName { get; set; } = string.Empty;
    public DateTime WeekStart { get; set; }   // UTC midnight of the first (Monday) column
    public int OpenHour { get; set; }
    public int CloseHour { get; set; }
    public List<SlotCellResponse> Cells { get; set; } = [];

    private Dictionary<(int Day, int Hour), SlotCellResponse>? _lookup;

    public IEnumerable<int> Hours => Enumerable.Range(OpenHour, Math.Max(0, CloseHour - OpenHour));

    public DateTime DateForColumn(int dayIndex) => WeekStart.AddDays(dayIndex);

    public SlotCellResponse? CellAt(int dayIndex, int hour)
    {
        _lookup ??= Cells
            .GroupBy(c => (c.DayIndex, c.Hour))
            .ToDictionary(g => g.Key, g => g.First());
        return _lookup.TryGetValue((dayIndex, hour), out var cell) ? cell : null;
    }
}

// Pending requests queue (spec 6 "Requests queue").
public class AppointmentRequestListViewModel
{
    public List<AppointmentRequestResponse> Requests { get; set; } = [];
    public List<TherapistResponse> Therapists { get; set; } = [];
    public Guid? TherapistFilter { get; set; }
}
