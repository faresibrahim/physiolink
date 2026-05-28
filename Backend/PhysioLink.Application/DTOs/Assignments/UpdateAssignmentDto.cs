using PhysioLink.Domain.Enums;

namespace PhysioLink.Application.DTOs.Assignments
{
    public class UpdateAssignmentDto
    {
        public required string TherapistName { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime ScheduledDate { get; set; }
        public int FrequencyPerWeek { get; set; }
        public AssignmentStatus Status { get; set; }
    }
}
