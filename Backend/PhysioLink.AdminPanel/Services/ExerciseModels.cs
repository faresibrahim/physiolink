using System.Text.Json.Serialization;

namespace PhysioLink.AdminPanel.Services;

public class ExerciseResponse
{
    [JsonPropertyName("exerciseId")]      public Guid ExerciseId { get; set; }
    [JsonPropertyName("name")]            public string Name { get; set; } = string.Empty;
    [JsonPropertyName("sets")]            public int Sets { get; set; }
    [JsonPropertyName("reps")]            public int Reps { get; set; }
    [JsonPropertyName("durationMinutes")] public int DurationMinutes { get; set; }
    [JsonPropertyName("description")]     public string Description { get; set; } = string.Empty;
    [JsonPropertyName("videoUrl")]        public string? VideoUrl { get; set; }
    [JsonPropertyName("difficulty")]      public string Difficulty { get; set; } = string.Empty;
    [JsonPropertyName("category")]        public string Category { get; set; } = string.Empty;
}

public class CreateExerciseRequest
{
    public required string Name { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public int DurationMinutes { get; set; }
    public required string Description { get; set; }
    public string? VideoUrl { get; set; }
    public string Difficulty { get; set; } = string.Empty;
}
