using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Exercises;


namespace PhysioLink.Application.Interfaces
{
    public interface IExerciseRepository
    {
        public Task<PagedResult<AssignedExerciseDto>> GetPatientExercisesAsync(int pageSize, int page, Guid patientId);
        public Task<bool> SubmitFeedbackAsync(Guid id, SubmitFeedbackDto request);
    }
}