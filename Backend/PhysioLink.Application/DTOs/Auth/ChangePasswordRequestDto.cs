namespace PhysioLink.Application.DTOs.Auth
{
    public class ChangePasswordRequestDto
    {
        // Optional: only required for a voluntary change (a user who is NOT on a
        // temporary password). First-time patients already authenticated with the
        // temporary password at login, so the client omits it.
        public string? CurrentPassword { get; set; }

        public string NewPassword { get; set; }
    }
}
