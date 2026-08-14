namespace PhysioLink.Application.DTOs
{
    public class DashboardDto
    {
        public int PatientCount { get; set; }
        public int TherapistCount { get; set; }
        public int ActiveAssignmentCount { get; set; }
        public int AppointmentsToday { get; set; }
    }
}
