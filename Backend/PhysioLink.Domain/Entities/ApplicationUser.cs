namespace PhysioLink.Domain.Entities
{
    public class ApplicationUser : AuditableEntity
    {
        public Guid ApplicationUserId {get; set;}
        public string FirstName {get; set;}
        public string LastName { get; set;}
        public string Email {get; set;}
        public string PasswordHash { get; set;}
        public string? RefreshToken {get; set;}
        public string Role { get; set; }
        public Guid ClinicId { get; set; }

        //??
        public DateTime? RefreshTokenExpiry {get; set;}

        public ApplicationUser (string firstName, string lastName, string email, string passwordHash, string role, Guid clinicId )
        {
            ApplicationUserId = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
            ClinicId = clinicId;
        }
    }
}