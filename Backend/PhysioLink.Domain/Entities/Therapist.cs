
namespace PhysioLink.Domain.Entities
{
    public class Therapist : ClinicScopedEntity
    {
        public Guid TherapistId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Speciality { get; set; }
        public bool IsActive { get; set; }
       

       public Therapist(Guid clinicId, string firstName, string lastName, string email, string phoneNumber, string speciality, bool isActive)
        {
            TherapistId = Guid.NewGuid();
            ClinicId = clinicId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            Speciality = speciality;
            IsActive = isActive;
        }
    }
}