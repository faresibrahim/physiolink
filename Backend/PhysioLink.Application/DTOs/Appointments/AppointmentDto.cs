using PhysioLink.Domain.Enums;

namespace PhysioLink.Application.DTOs
{
    public class AppointmentDto
    {
        public Guid AppointmentId { get; set; }
        public Guid PatientId { get; set; }
        public Guid TherapistId { get; set; }
        public string Title { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTime AppointmentTime { get; set; }
        public AppointmentStatus AppointmentStatus { get; set; } = AppointmentStatus.Pending;
        public string PatientName { get; set; } = null!;
    }
}
