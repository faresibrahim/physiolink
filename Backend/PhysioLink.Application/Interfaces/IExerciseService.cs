using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Exercises;

namespace PhysioLink.Application.Interfaces
{
    public interface IExerciseService
    {
        Task<PagedResult<AssignedExerciseDto>> GetPatientExercisesAsync(Guid patientId, int page, int pageSize);
        // callerPatientId scopes the feedback to the caller's own assignment; a
        // mismatch returns false so a patient can't rate another patient's exercise.
        Task<bool> SubmitFeedbackAsync(Guid id, SubmitFeedbackDto request, Guid callerPatientId);
    }
}