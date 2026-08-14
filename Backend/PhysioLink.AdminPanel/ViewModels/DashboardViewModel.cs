using PhysioLink.AdminPanel.Services;

namespace PhysioLink.AdminPanel.ViewModels
{
    public class DashboardViewModel
    {
        public int PatientCount { get; set; }
        public int TherapistCount { get; set; }
        public int ActiveAssignment { get; set; }
        public int TodayAppointments { get; set; }
        public List<AppointmentSummaryResponse> UpcomingAppointments { get; set; } = [];
        public List<PatientResponse> RecentPatients { get; set; } = [];
    }
}
