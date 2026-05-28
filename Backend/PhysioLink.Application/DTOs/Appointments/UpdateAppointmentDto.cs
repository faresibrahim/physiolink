using PhysioLink.Domain.Enums;

namespace PhysioLink.Application.DTOs.Appointments
{
    public class UpdateAppointmentDto
    {
        public required string Title { get; set; }
        public string? Notes { get; set; }
        public string? TherapistName { get; set; }
        public DateTime AppointmentTime { get; set; }
        public AppointmentStatus Status { get; set; }
    }
}
