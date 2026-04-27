using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Exercises;

namespace PhysioLink.Application.Interfaces
{
    public interface IExerciseService
    {
        Task<PagedResult<AssignedExerciseDto>> GetPatientExercisesAsync(Guid patientId, int page, int pageSize);
        Task<bool> SubmitFeedbackAsync(Guid id,SubmitFeedbackDto request);
    }
}