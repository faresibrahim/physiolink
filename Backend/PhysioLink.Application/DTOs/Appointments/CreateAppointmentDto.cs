namespace PhysioLink.Application.DTOs.Appointments
{
    public class CreateAppointmentDto
    {
        public required string Title { get; set; }
        public string? Notes { get; set; }
        public Guid PatientId { get; set; }
        public string? TherapistName { get; set; }
        public DateTime AppointmentTime { get; set; }
    }
}
