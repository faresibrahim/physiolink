using PhysioLink.Domain.Enums;

namespace PhysioLink.Application.DTOs.Appointments
{
    public class UpdateAppointmentDto
    {
        public required string Title { get; set; }
        public string? Notes { get; set; }
        // Both null = leave unchanged. The admin panel locks (and so omits) these
        // fields once an appointment is past Requested.
        public string? TherapistName { get; set; }
        public DateTime? AppointmentTime { get; set; }
        public AppointmentStatus Status { get; set; }
    }
}
