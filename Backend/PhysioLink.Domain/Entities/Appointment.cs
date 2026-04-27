using PhysioLink.Domain.Enums;

namespace PhysioLink.Domain.Entities
{
    public class Appointment : AuditableEntity
    {
        public Guid AppointmentId { get; set;}
        public string Title { get; set; } = "Appointment Request";
        public string? Notes { get; set; }
        public Guid PatientId { get; set;}
        public Patient Patient {get; set;}
        public Guid TherapistId { get; set;}
        public DateTime AppointmentTime {get; set; }
        public AppointmentStatus Status { get; set; }

        public Appointment(AppointmentStatus status, DateTime appointmentTime, Guid patientId, Guid therapistId, string? notes = null)
        {
            PatientId = patientId;
            TherapistId = therapistId;
            AppointmentTime = appointmentTime;
            Notes = notes;
            AppointmentId = Guid.NewGuid();
            Status = status;
        }
    }
}
