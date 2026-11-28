using PhysioLink.Application.DTOs.Appointments;

namespace PhysioLink.Application.DTOs.Slots
{
    public enum RescheduleOutcome
    {
        Requested,        // confirmed appt -> new linked request created (201)
        Moved,            // pending request moved to the new slot in place (200)
        NotFound,         // no such appointment for this patient (404)
        SlotUnavailable,  // slot taken / not their therapist / in the past (409)
        TooLate,          // inside the cutoff before the session starts (422)
        NotReschedulable  // wrong status, or a reschedule is already pending (422)
    }

    public class RescheduleResult
    {
        // Patients can't move a session in the app inside this window; they call the clinic.
        public static readonly TimeSpan Cutoff = TimeSpan.FromHours(12);

        public RescheduleOutcome Outcome { get; set; }
        public PatientAppointmentDto? Appointment { get; set; }

        public static RescheduleResult Of(RescheduleOutcome outcome, PatientAppointmentDto? appt = null) =>
            new() { Outcome = outcome, Appointment = appt };
    }
}
