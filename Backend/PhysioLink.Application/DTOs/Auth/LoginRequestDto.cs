

namespace PhysioLink.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        // ClinicAdmin clients (Admin Panel) log in with Email.
        public string? Email {get; set;}

        // Patient clients (Flutter app) log in with Username.
        public string? Username {get; set;}

        public string Password {get; set;}

    }
}