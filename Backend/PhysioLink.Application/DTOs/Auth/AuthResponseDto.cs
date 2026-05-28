
namespace PhysioLink.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string AccessToken {get; set;}
        public string RefreshToken {get; set;}
        public Guid PatientId { get; set; }
        public string? ClinicName { get; set; }
    }
}