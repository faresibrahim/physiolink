using PhysioLink.Domain.Enums;

namespace PhysioLink.Application.DTOs.Exercises
{
    public class AdminExerciseDto
    {
        public Guid ExerciseId { get; set; }
        public required string Name { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public int DurationMinutes { get; set; }
        public required string Description { get; set; }
        public string? VideoUrl { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public ExerciseCategory Category { get; set; }
    }
}
