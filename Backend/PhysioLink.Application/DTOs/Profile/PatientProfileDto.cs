namespace PhysioLink.Application.DTOs.Profile
{
    public class PatientProfileDto
    {
        public Guid PatientId {get; set;}
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PhoneNumber {get; set;} = null!;
        public string? Email {get; set;}

        public string Diagnosis {get; set;} = null!;
        public string? TherapistName { get; set; }
        public string? ClinicName { get; set; }

    }
}