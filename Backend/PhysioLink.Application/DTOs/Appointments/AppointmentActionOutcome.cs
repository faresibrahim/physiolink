namespace PhysioLink.Application.DTOs.Appointments
{
    // Outcome of an accept/reject/cancel transition so the controller maps the HTTP
    // status (spec 4.2-4.4): NotFound -> 404, InvalidState -> 409.
    public enum AppointmentActionOutcome
    {
        Ok,
        NotFound,
        InvalidState
    }
}
