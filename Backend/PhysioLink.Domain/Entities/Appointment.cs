using PhysioLink.Domain.Enums;

namespace PhysioLink.Domain.Entities
{
    public class Appointment : ClinicScopedEntity
    {
        public Guid AppointmentId { get; set; }
        public string Title { get; set; } = "Appointment Request";
        public string? Notes { get; set; }
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        // Owner of the appointment. Non-null going forward (a booked appt always has
        // a therapist). Kept as a snapshot alongside TherapistName so a later
        // rename/soft-delete of the therapist doesn't rewrite history (spec 1.4).
        public Guid TherapistId { get; set; }
        public string TherapistName { get; set; } = null!;

        // Loose, nullable link to the slot this appointment came from. Nullable
        // because a CancelledByClinic / Rejected / Expired appt may outlive its
        // freed slot (spec 1.4 / D2).
        public Guid? AppointmentSlotId { get; set; }
        public AppointmentSlot? AppointmentSlot { get; set; }

        public DateTime AppointmentTime { get; set; }
        public AppointmentStatus Status { get; set; }

        // EF + object-initializer construction (the slot-based request flow).
        public Appointment()
        {
            AppointmentId = Guid.NewGuid();
        }

        // Legacy constructor retained for backward compatibility with the old
        // direct-create path.
        public Appointment(AppointmentStatus status, DateTime appointmentTime, Guid patientId, string therapistName, Guid clinicId, string? notes = null)
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
