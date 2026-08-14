using PhysioLink.Application.DTOs.Appointments;

namespace PhysioLink.Application.DTOs.Slots
{
    public enum RequestSlotOutcome
    {
        Created,          // won the slot -> 201 with the pending appointment
        Conflict,         // lost the race / not their therapist / past / unassigned -> 409
        PatientNotFound   // no patient for the caller's identity -> 404
    }

    public class RequestSlotResult
    {
        public RequestSlotOutcome Outcome { get; set; }
        public PatientAppointmentDto? Appointment { get; set; }

        public static RequestSlotResult Conflict() => new() { Outcome = RequestSlotOutcome.Conflict };
        public static RequestSlotResult PatientNotFound() => new() { Outcome = RequestSlotOutcome.PatientNotFound };
        public static RequestSlotResult Created(PatientAppointmentDto appt) =>
            new() { Outcome = RequestSlotOutcome.Created, Appointment = appt };
    }
}
