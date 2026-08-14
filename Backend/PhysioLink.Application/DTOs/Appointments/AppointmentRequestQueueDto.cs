using PhysioLink.Domain.Enums;

namespace PhysioLink.Application.DTOs.Appointments
{
    // A pending request as shown in the admin decision queue (spec 4.1).
    public class AppointmentRequestQueueDto
    {
        public Guid AppointmentId { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid TherapistId { get; set; }
        public string TherapistName { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public string Type { get; set; } = string.Empty;   // appointment Title
        public string? Notes { get; set; }
        public DateTime RequestedAt { get; set; }           // CreatedAt = request time
        public AppointmentStatus Status { get; set; }
    }
}
