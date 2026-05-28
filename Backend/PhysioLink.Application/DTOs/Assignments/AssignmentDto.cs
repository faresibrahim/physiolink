using PhysioLink.Domain.Enums;

namespace PhysioLink.Application.DTOs.Assignments
{
    public class AssignmentDto
    {
        public Guid ExerciseAssignmentId { get; set; }
        public Guid PatientId { get; set; }
        public Guid ExerciseId { get; set; }
        public required string ExerciseName { get; set; }
        public required string TherapistName { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime ScheduledDate { get; set; }
        public int FrequencyPerWeek { get; set; }
        public AssignmentStatus Status { get; set; }
        public int? Feedback { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
