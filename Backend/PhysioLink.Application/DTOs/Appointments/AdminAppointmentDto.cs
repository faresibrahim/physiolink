using PhysioLink.Domain.Enums;

namespace PhysioLink.Application.DTOs.Appointments
{
    public class AdminAppointmentDto
    {
        public Guid AppointmentId { get; set; }
        public required string Title { get; set; }
        public string? Notes { get; set; }
        public Guid PatientId { get; set; }
        public required string PatientName { get; set; }
        public string? TherapistName { get; set; }
        public DateTime AppointmentTime { get; set; }
        public AppointmentStatus Status { get; set; }
    }
}
