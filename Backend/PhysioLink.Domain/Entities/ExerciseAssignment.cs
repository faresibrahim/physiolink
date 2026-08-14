using PhysioLink.Domain.Enums;
namespace PhysioLink.Domain.Entities
{
    public class ExerciseAssignment : AuditableEntity
    {
        public Guid ExerciseAssignmentId { get; set;}
        public string TherapistName { get; set;}
        public Guid PatientId { get; set;}
        public Guid ExerciseId { get; set;}
        public Exercise Exercise {get; set;}
        public int Sets { get; set;}
        public int Reps { get; set;}
        public int DurationMinutes { get; set;}
        public int FrequencyPerWeek { get; set; }
        public int? FrequencyPerDay { get; set; }
        public AssignmentStatus Status { get; set; }

        public int? Feedback { get; set;}
        public DateTime? CompletedAt { get; set; }
        public DateTime AssignedAt {get; set;}

        public ExerciseAssignment(string therapistName, Guid patientId, Guid exerciseId,
        int sets, int reps, int durationMinutes, int frequencyPerWeek, int? frequencyPerDay, DateTime assignedAt, int? feedback = null)
        {
         ExerciseAssignmentId = Guid.NewGuid();
         PatientId = patientId;
         TherapistName = therapistName;
         ExerciseId = exerciseId;
         Sets = sets;
         Reps = reps;
         DurationMinutes = durationMinutes;
         FrequencyPerWeek = frequencyPerWeek;
         FrequencyPerDay = frequencyPerDay;
         Status = AssignmentStatus.Active;
         Feedback = feedback;
         AssignedAt = assignedAt;
        }
    }
}