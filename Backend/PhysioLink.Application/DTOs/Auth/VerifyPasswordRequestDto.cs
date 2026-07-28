

namespace PhysioLink.Application.DTOs.Auth
{
    // Re-authentication payload for destructive actions (e.g. deactivating a patient).
    // The account is taken from the caller's token — only the password is submitted.
    public class VerifyPasswordRequestDto
    {
        public string Password {get; set;}
    }
}
