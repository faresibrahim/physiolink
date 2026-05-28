using System.Text.Json.Serialization;

namespace PhysioLink.AdminPanel.Services;

public class AssignExerciseRequest
{
    public required Guid ExerciseId { get; set; }
    public required string TherapistName { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime ScheduledDate { get; set; }
    public int FrequencyPerWeek { get; set; }
}

public class UpdateAssignmentRequest
{
    public required string TherapistName { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime ScheduledDate { get; set; }
    public int FrequencyPerWeek { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class AssignmentResponse
{
    [JsonPropertyName("exerciseAssignmentId")] public Guid ExerciseAssignmentId { get; set; }
    [JsonPropertyName("patientId")]            public Guid PatientId { get; set; }
    [JsonPropertyName("exerciseId")]           public Guid ExerciseId { get; set; }
    [JsonPropertyName("exerciseName")]         public string ExerciseName { get; set; } = string.Empty;
    [JsonPropertyName("therapistName")]        public string TherapistName { get; set; } = string.Empty;
    [JsonPropertyName("sets")]                 public int Sets { get; set; }
    [JsonPropertyName("reps")]                 public int Reps { get; set; }
    [JsonPropertyName("durationMinutes")]      public int DurationMinutes { get; set; }
    [JsonPropertyName("scheduledDate")]        public DateTime ScheduledDate { get; set; }
    [JsonPropertyName("frequencyPerWeek")]     public int FrequencyPerWeek { get; set; }
    [JsonPropertyName("status")]               public string Status { get; set; } = string.Empty;
    [JsonPropertyName("feedback")]             public int? Feedback { get; set; }
}
