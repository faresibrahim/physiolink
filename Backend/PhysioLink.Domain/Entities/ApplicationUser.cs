namespace PhysioLink.Domain.Entities
{
    public class ApplicationUser : AuditableEntity
    {
        public Guid ApplicationUserId {get; set;}
        public string FirstName {get; set;}
        public string LastName { get; set;}
        public string? Email {get; set;}

        // Login identifier for Patient-role accounts. Null for ClinicAdmin rows,
        // which still log in with Email.
        public string? Username { get; set; }
        public string PasswordHash { get; set;}
        public string? RefreshToken {get; set;}
        public string Role { get; set; }
        public Guid ClinicId { get; set; }

        // True while the account is still on the admin-issued temporary password.
        // The patient must set their own password on first login; cleared once they do.
        public bool MustChangePassword { get; set; }

        //??
        public DateTime? RefreshTokenExpiry {get; set;}

        public ApplicationUser (string firstName, string lastName, string? email, string passwordHash, string role, Guid clinicId )
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