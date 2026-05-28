using PhysioLink.Domain.Enums;

namespace PhysioLink.Application.DTOs.Patients
{
    public class AppointmentSummaryDto
    {
        public Guid AppointmentId { get; set; }
        public required string Title { get; set; }
        public DateTime AppointmentTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? Notes { get; set; }
        public Guid PatientId { get; set; }
        public string TherapistName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
    }
}
