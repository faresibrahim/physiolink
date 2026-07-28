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

        /// <summary>
        /// Arabic translation of <see cref="Description"/> (the steps to perform).
        /// Null when no translation has been provided yet — clients fall back to
        /// the English <see cref="Description"/>.
        /// </summary>
        public string? DescriptionAr { get; set; }
        public string? VideoUrl { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public ExerciseCategory Category { get; set; }

        public Exercise(string name, int reps, int sets, int durationMinutes,
            string description, DifficultyLevel difficulty, ExerciseCategory category,
            string? descriptionAr = null)
        {
            ExerciseId = Guid.NewGuid();
            Reps = reps;
            Sets = sets;
            Name = name;
            DurationMinutes = durationMinutes;
            Description = description;
            DescriptionAr = descriptionAr;
            Difficulty = difficulty;
            Category = category;
        }
    }
}
