using PhysioLink.Domain.Enums;

namespace PhysioLink.Domain.Entities
{
    public class Exercise : AuditableEntity
    {
        public Guid ExerciseId { get; set; }
        public string Name { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; }
        public string? VideoUrl { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public ExerciseCategory Category { get; set; }

        public Exercise(string name, int reps, int sets, int durationMinutes,
            string description, DifficultyLevel difficulty, ExerciseCategory category)
        {
            ExerciseId = Guid.NewGuid();
            Reps = reps;
            Sets = sets;
            Name = name;
            DurationMinutes = durationMinutes;
            Description = description;
            Difficulty = difficulty;
            Category = category;
        }
    }
}
