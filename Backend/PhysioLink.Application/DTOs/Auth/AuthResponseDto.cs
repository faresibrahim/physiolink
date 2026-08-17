
namespace PhysioLink.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string AccessToken {get; set;}
        public string RefreshToken {get; set;}
        public Guid PatientId { get; set; }
        public string? ClinicName { get; set; }

        // The account's role (e.g. "ClinicAdmin", "Patient"). Lets each client admit
        // only the roles it's built for — the admin panel rejects non-admins.
        public string? Role { get; set; }

        // When true, the client must route the patient to the change-password screen
        // before granting access — they are still on the admin-issued temporary password.
        public bool MustChangePassword { get; set; }
    }
}