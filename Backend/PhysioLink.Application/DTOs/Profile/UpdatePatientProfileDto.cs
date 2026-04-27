namespace PhysioLink.Application.DTOs.Patients
{
    public class UpdatePatientProfileDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Diagnosis { get; set; }
    }
}