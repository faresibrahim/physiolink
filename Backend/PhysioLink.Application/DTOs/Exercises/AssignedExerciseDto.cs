using PhysioLink.Domain.Enums;
namespace PhysioLink.Application.DTOs.Exercises
{
    public class AssignedExerciseDto
    {
        public Guid ExerciseId {get; set;}
        public Guid ExerciseAssignmentId {get; set;}
        public string ExerciseName {get; set;} = null!; 
        public int Sets {get; set;}
        public int Reps{get; set;}
        public int DurationMinutes {get; set;}

        public string? Description {get; set;}

        public string? DescriptionAr {get; set;}

        public string? VideoUrl {get; set;}

        public DifficultyLevel Difficulty {get; set;}

        public int? Feedback {get; set;}
        public DateTime? CompletedAt { get; set; }
        public DateTime AssignedAt {get; set;}

    }
}