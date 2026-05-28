using System.Text.Json.Serialization;

namespace PhysioLink.AdminPanel.Services
{
    public class DashboardStatsResponse
    {
        [JsonPropertyName("patientCount")]
        public int PatientCount { get; set; }

        [JsonPropertyName("therapistCount")]
        public int TherapistCount { get; set; }

        [JsonPropertyName("activeAssignmentCount")]
        public int ActiveAssignmentCount { get; set; }

        [JsonPropertyName("appointmentsThisWeek")]
        public int AppointmentsThisWeek { get; set; }
    }
}
