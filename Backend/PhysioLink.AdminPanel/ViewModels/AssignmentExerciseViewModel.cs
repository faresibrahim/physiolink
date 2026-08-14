namespace PhysioLink.AdminPanel.ViewModels
{
    public class AssignmentExerciseViewModel
    {
        public Guid PatientId { get; set; }
        public Guid ExerciseId { get; set; }
        public required string TherapistName { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public int DurationMinutes { get; set; }
        public int FrequencyPerWeek { get; set; }
        public int? FrequencyPerDay { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class EditAssignmentViewModel
    {
        public Guid ExerciseAssignmentId { get; set; }
        public Guid PatientId { get; set; }
        // Display only — shown in the form header, not editable
        public string ExerciseName { get; set; } = string.Empty;
        public string TherapistName { get; set; } = string.Empty;
        public int Sets { get; set; }
        public int Reps { get; set; }
        public int DurationMinutes { get; set; }
        public int FrequencyPerWeek { get; set; }
        public int? FrequencyPerDay { get; set; }
        public string Status { get; set; } = "Active";
    }
}
