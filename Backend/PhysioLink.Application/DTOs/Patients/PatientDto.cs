namespace PhysioLink.Application.DTOs.Patients
{
    public class PatientDto
    {
        public Guid PatientId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Diagnosis { get; set; }
        public string? TemporaryPassword { get ; set; }
        public Guid? TherapistId { get; set; }
        public string? TherapistName { get; set; }
        public bool IsActive { get; set; }
    }
}