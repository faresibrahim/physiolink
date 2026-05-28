namespace PhysioLink.Application.DTOs.Therapists
{
    public class CreateTherapistDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Speciality { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        //IsActive property is not added here. Therapist is set to active by default
    }
}