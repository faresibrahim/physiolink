using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Exercises;
using PhysioLink.Application.Interfaces;

namespace PhysioLink.Application.Services
{
   
    public class ExerciseService : IExerciseService
    {
         private readonly IExerciseRepository _exerciseRepository;

         public ExerciseService (IExerciseRepository exerciseRepository)
        {
            _exerciseRepository = exerciseRepository;
        }  


        public async Task<PagedResult<AssignedExerciseDto>> GetPatientExercisesAsync(Guid patientId, int page, int pageSize)
        {
            return await _exerciseRepository.GetPatientExercisesAsync(pageSize, page,  patientId);
        }

        public async Task<bool> SubmitFeedbackAsync (Guid id, SubmitFeedbackDto level)
        {
            return await _exerciseRepository.SubmitFeedbackAsync(id, level);
        }
    }
}