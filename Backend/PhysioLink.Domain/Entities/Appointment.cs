using PhysioLink.Domain.Enums;

namespace PhysioLink.Domain.Entities
{
    public class Appointment : ClinicScopedEntity
    {
        public Guid AppointmentId { get; set;}
        public string Title { get; set; } = "Appointment Request";
        public string? Notes { get; set; }
        public Guid PatientId { get; set;}
        public Patient Patient {get; set;}
        public string TherapistName { get; set;}
        public DateTime AppointmentTime {get; set; }
        public AppointmentStatus Status { get; set; }
        

        public Appointment(AppointmentStatus status, DateTime appointmentTime, Guid patientId,string therapistName, Guid clinicId, string? notes = null)
        {
            PatientId = patientId;
            TherapistName = therapistName;
            ClinicId = clinicId;
            AppointmentTime = appointmentTime;
            Notes = notes;
            AppointmentId = Guid.NewGuid();
            Status = status;
        }
    }
}
