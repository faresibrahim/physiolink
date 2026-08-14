namespace PhysioLink.Application.DTOs.Appointments
{
    // Patient-facing appointment view. Carries both the raw Status (for the app to
    // switch on) and an explicit StatusLabel so "pending, not confirmed" is
    // unmistakable across every surface (spec 3.2 / Phase 7).
    public class PatientAppointmentDto
    {
        public Guid AppointmentId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string TherapistName { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;      // enum name, e.g. "Requested"
        public string StatusLabel { get; set; } = string.Empty; // honest wording for display
        public Guid? SlotId { get; set; }
    }
}
