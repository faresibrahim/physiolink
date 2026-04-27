namespace PhysioLink.Application.DTOs {
public class PatientProgressDto
{
    public int TotalAssignedExercises { get; set; }
    public int ExercisesWithFeedback { get; set; }
    public int TotalAppointments { get; set; }
    public DateTime? LastAppointmentDate { get; set; }
}
}