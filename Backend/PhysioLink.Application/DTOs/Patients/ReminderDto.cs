namespace PhysioLink.Application.DTOs.Patients
{
    // The patient's daily exercise reminder. Time is "HH:mm", clinic local time.
    public class ReminderDto
    {
        public bool Enabled { get; set; }
        public string Time { get; set; } = "18:00";
    }
}
