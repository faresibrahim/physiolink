using System.Text.Json.Serialization;

namespace PhysioLink.AdminPanel.Services
{
    public class TherapistResponse
    {
        [JsonPropertyName("therapistId")]
        public Guid Id { get; set; }
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;
        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;
        [JsonPropertyName("speciality")]
        public string Specialty { get; set; } = string.Empty;
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }

    // Request models do not need [JsonPropertyName] attributes.
    // System.Text.Json serializes outgoing JSON using property names as-is.
    // The HttpClient is configured with JsonNamingPolicy.CamelCase, which
    // automatically converts PascalCase property names to camelCase on serialization.
    // [JsonPropertyName] is only needed on response models for deserialization matching.
    public class CreateTherapistRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Speciality { get; set; }
    }

    public class UpdateTherapistRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Speciality { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
