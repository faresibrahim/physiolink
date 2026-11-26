namespace PhysioLink.Application.DTOs.Patients
{
    // Time is a 24-hour "HH:mm" string in CLINIC local time. No time zone is
    // accepted from the client — see UpdateReminderValidator.
    public class UpdateReminderDto
    {
        public bool Enabled { get; set; }
        public required string Time { get; set; }
    }
}
