using PhysioLink.Domain.Enums;

namespace PhysioLink.Application.DTOs.Patients
{
    public class AssignmentSummaryDto
    {
        public Guid ExerciseAssignmentId { get; set; }
        public required string ExerciseName { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public int DurationMinutes { get; set; }
        public int? Feedback { get; set; }
        public AssignmentStatus Status { get; set; }
    }
}
