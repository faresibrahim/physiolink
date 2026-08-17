using System.Text.Json.Serialization;

namespace PhysioLink.AdminPanel.Services
{
    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
    public class LoginResponse
    {
        [JsonPropertyName("accessToken")]
        public string? AccessToken { get; set; }
        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; set; }
        [JsonPropertyName("clinicName")]
        public string? ClinicName { get; set; }
        [JsonPropertyName("role")]
        public string? Role { get; set; }
    }
}
