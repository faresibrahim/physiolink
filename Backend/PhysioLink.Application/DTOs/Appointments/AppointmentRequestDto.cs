namespace PhysioLink.Application.DTOs.Appointments
{
    public class AppointmentRequestDto
    {
        public Guid PatientId { get; set; }
        public DateTime AppointmentTime { get; set; }
        public string? TherapistName { get; set; }
        public string? Notes { get; set; }
    }
}
