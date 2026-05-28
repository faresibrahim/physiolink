namespace PhysioLink.Application.DTOs.Therapists
{
    public class UpdateTherapistDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Speciality { get; set; }
        public bool IsActive { get; set; }
    }
}