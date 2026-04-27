
namespace PhysioLink.Domain.Entities
{
    public class ExerciseAssignment : AuditableEntity
    {
        public Guid ExerciseAssignmentId { get; set;}
        public Guid TherapistId { get; set;}
        public Guid PatientId { get; set;}
        public Guid ExerciseId { get; set;}
        public Exercise Exercise {get; set;}
        public int Sets { get; set;}
        public int Reps { get; set;}
        public int DurationMinutes { get; set;}

        public int? Feedback { get; set;}
        public DateTime? CompletedAt { get; set; }
        public DateTime AssignedAt {get; set;}

        public ExerciseAssignment(Guid therapistId, Guid patientId, Guid exerciseId,
        int sets, int reps, int durationMinutes, DateTime assignedAt, int? feedback = null)
        {
         ExerciseAssignmentId = Guid.NewGuid();
         PatientId = patientId;
         TherapistId = therapistId;
         ExerciseId = exerciseId;
         Sets = sets;
         Reps = reps;
         DurationMinutes = durationMinutes;   
         Feedback = feedback;
         AssignedAt = assignedAt;
        }
    }
}