using Microsoft.EntityFrameworkCore;
using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Exercises;
using PhysioLink.Application.Interfaces;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Repositories
{
    public class ExerciseRepository : IExerciseRepository
    {
       private readonly PhysioLinkDbContext _dbContext;

       public ExerciseRepository(PhysioLinkDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<AssignedExerciseDto>> GetPatientExercisesAsync(int pageSize, int page, Guid patientId)
        {

            var totalCount = await _dbContext.ExerciseAssignments
            .Where(p => p.PatientId == patientId)
            .CountAsync();

            var items = await _dbContext.ExerciseAssignments.Where(p=>p.PatientId == patientId)
            .Include(p=>p.Exercise)
            .AsNoTracking()
            .Select(ea => new AssignedExerciseDto
            {
                ExerciseName = ea.Exercise.Name,
                ExerciseId = ea.ExerciseId,
                ExerciseAssignmentId = ea.ExerciseAssignmentId,
                Sets = ea.Sets,
                Reps = ea.Reps,
                DurationMinutes = ea.DurationMinutes,
                Description = ea.Exercise.Description,
                DescriptionAr = ea.Exercise.DescriptionAr,
                VideoUrl = ea.Exercise.VideoUrl,
                Difficulty = ea.Exercise.Difficulty,
                Feedback = ea.Feedback,
                CompletedAt = ea.CompletedAt,
                AssignedAt = ea.AssignedAt,

            })
            .Skip((page-1) * pageSize)
            .Take(pageSize).ToListAsync();

            return new PagedResult<AssignedExerciseDto>
            {
                Items = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecordCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<bool> SubmitFeedbackAsync(Guid id, SubmitFeedbackDto request, Guid callerPatientId)
        {
            // Scope the lookup to the caller's own assignment: a valid assignment id
            // that belongs to another patient returns null here (treated as not found),
            // so feedback can only be written to the caller's own exercises.
            var assignment = await  _dbContext.ExerciseAssignments
                .FirstOrDefaultAsync(p => p.ExerciseAssignmentId == id && p.PatientId == callerPatientId);
            if (assignment == null)
                return false;

            
            assignment.Feedback = request.Rating;
            assignment.CompletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }

      
    }
}