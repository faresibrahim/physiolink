using System.Text.Json.Serialization;



namespace PhysioLink.AdminPanel.Services;

    public class PatientResponse
    {
        [JsonPropertyName("patientId")]
        public Guid PatientId { get; set; }
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;
        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;
        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;
        [JsonPropertyName("diagnosis")]
        public string Diagnosis { get; set; } = string.Empty;
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("temporaryPassword")]
        public string? TemporaryPassword { get; set; }
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
        [JsonPropertyName("therapistName")]
        public string? TherapistName { get; set; }
    }

public class CreatePatientRequest
    {
    //Only response classes require JsonPropertyName attribute because they are mapped by the API's json response.
    //the properties names need to exactly match the API's deserialized response
    //Request models are serialized outbound by System.Text.Json using JsonNamingPolicy.
    //CamelCase configured on the HttpClient,
    //which automatically converts PascalCase property names to camelCase
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Diagnosis { get; set; }
        public required string Email { get; set; }
        public Guid? TherapistId { get; set; }
    }

    public class UpdatePatientRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Diagnosis { get; set; }
        public Guid? TherapistId { get; set; }  
        public bool IsActive { get; set; }
    }

    public class PatientDetailResponse
    {
        [JsonPropertyName("patientId")]
        public Guid PatientId { get; set; }
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;
        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;
        [JsonPropertyName("therapistName")]
        public string? TherapistName { get; set; }
        [JsonPropertyName("clinicName")]
        public string ClinicName { get; set; } = string.Empty;
        [JsonPropertyName("diagnosis")]
        public string Diagnosis { get; set; } = string.Empty;
        [JsonPropertyName("exercises")]
        public List<AssignmentSummaryResponse> Exercises { get; set; } = [];
        [JsonPropertyName("appointments")]
        public List<AppointmentSummaryResponse> Appointments { get; set; } = [];
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }

public class AssignmentSummaryResponse
{
    [JsonPropertyName("exerciseAssignmentId")]
    public Guid ExerciseAssignmentId { get; set; }
    [JsonPropertyName("exerciseName")]
    public string ExerciseName { get; set; } = string.Empty;
    [JsonPropertyName("sets")]
    public int Sets { get; set; }
    [JsonPropertyName("reps")]
    public int Reps { get; set; }
    [JsonPropertyName("durationMinutes")]
    public int DurationMinutes { get; set; }
    [JsonPropertyName("feedback")]
    public int? Feedback { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    [JsonPropertyName("scheduledDate")]
    public DateTime ScheduledDate { get; set; }
}

