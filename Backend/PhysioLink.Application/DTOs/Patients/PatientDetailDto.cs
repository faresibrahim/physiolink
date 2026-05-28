namespace PhysioLink.Application.DTOs.Patients
{
    public class PatientDetailDto
    {
        public Guid PatientId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public string? TherapistName { get; set; }
        public string ClinicName { get; set; } = string.Empty;
        public required string Diagnosis { get; set; }
        public required List<AssignmentSummaryDto> Exercises { get; set; }
        public required List<AppointmentSummaryDto> Appointments { get; set; }
        public bool IsActive { get; set; }
    }
}