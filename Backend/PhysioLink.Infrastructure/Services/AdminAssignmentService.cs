using Microsoft.EntityFrameworkCore;
using PhysioLink.Application.DTOs;
using PhysioLink.Application.DTOs.Assignments;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Entities;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Services
{
    public class AdminAssignmentService : IAdminAssignmentService
    {
        private readonly PhysioLinkDbContext _dbContext;

        public AdminAssignmentService(PhysioLinkDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<AssignmentDto>> GetAllByPatientAsync(Guid patientId, int page, int pageSize)
        {
            var query = _dbContext.ExerciseAssignments.AsNoTracking()
                .Where(ea => ea.PatientId == patientId)
                .Select(ea => new AssignmentDto
                {
                    ExerciseAssignmentId = ea.ExerciseAssignmentId,
                    PatientId = ea.PatientId,
                    ExerciseId = ea.ExerciseId,
                    ExerciseName = ea.Exercise.Name,
                    TherapistName = ea.TherapistName,
                    Sets = ea.Sets,
                    Reps = ea.Reps,
                    DurationMinutes = ea.DurationMinutes,
                    FrequencyPerWeek = ea.FrequencyPerWeek,
                    FrequencyPerDay = ea.FrequencyPerDay,
                    Status = ea.Status,
                    Feedback = ea.Feedback,
                    AssignedAt = ea.AssignedAt,
                    CompletedAt = ea.CompletedAt
                });

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<AssignmentDto>
            {
                Items = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecordCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<AssignmentDto?> GetByIdAsync(Guid id)
        {
            return await _dbContext.ExerciseAssignments.AsNoTracking()
                .Where(ea => ea.ExerciseAssignmentId == id)
                .Select(ea => new AssignmentDto
                {
                    ExerciseAssignmentId = ea.ExerciseAssignmentId,
                    PatientId = ea.PatientId,
                    ExerciseId = ea.ExerciseId,
                    ExerciseName = ea.Exercise.Name,
                    TherapistName = ea.TherapistName,
                    Sets = ea.Sets,
                    Reps = ea.Reps,
                    DurationMinutes = ea.DurationMinutes,
                    FrequencyPerWeek = ea.FrequencyPerWeek,
                    FrequencyPerDay = ea.FrequencyPerDay,
                    Status = ea.Status,
                    Feedback = ea.Feedback,
                    AssignedAt = ea.AssignedAt,
                    CompletedAt = ea.CompletedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<AssignmentDto> CreateAsync(Guid patientId, CreateAssignmentDto dto)
        {
            var exerciseName = await _dbContext.Exercises.AsNoTracking()
                .Where(e => e.ExerciseId == dto.ExerciseId)
                .Select(e => e.Name)
                .FirstOrDefaultAsync() ?? string.Empty;

            var assignment = new ExerciseAssignment(
                dto.TherapistName,
                patientId,
                dto.ExerciseId,
                dto.Sets,
                dto.Reps,
                dto.DurationMinutes,
                dto.FrequencyPerWeek,
                dto.FrequencyPerDay,
                assignedAt: DateTime.UtcNow
            );

            _dbContext.ExerciseAssignments.Add(assignment);
            await _dbContext.SaveChangesAsync();

            return new AssignmentDto
            {
                ExerciseAssignmentId = assignment.ExerciseAssignmentId,
                PatientId = assignment.PatientId,
                ExerciseId = assignment.ExerciseId,
                ExerciseName = exerciseName,
                TherapistName = assignment.TherapistName,
                Sets = assignment.Sets,
                Reps = assignment.Reps,
                DurationMinutes = assignment.DurationMinutes,
                FrequencyPerWeek = assignment.FrequencyPerWeek,
                FrequencyPerDay = assignment.FrequencyPerDay,
                Status = assignment.Status,
                Feedback = assignment.Feedback,
                AssignedAt = assignment.AssignedAt,
                CompletedAt = assignment.CompletedAt
            };
        }

        public async Task<AssignmentDto?> UpdateAsync(Guid id, UpdateAssignmentDto dto)
        {
            var assignment = await _dbContext.ExerciseAssignments
                .SingleOrDefaultAsync(ea => ea.ExerciseAssignmentId == id);

            if (assignment == null) return null;

            var exerciseName = await _dbContext.Exercises.AsNoTracking()
                .Where(e => e.ExerciseId == assignment.ExerciseId)
                .Select(e => e.Name)
                .FirstOrDefaultAsync() ?? string.Empty;

            assignment.TherapistName = dto.TherapistName;
            assignment.Sets = dto.Sets;
            assignment.Reps = dto.Reps;
            assignment.DurationMinutes = dto.DurationMinutes;
            assignment.FrequencyPerWeek = dto.FrequencyPerWeek;
            assignment.FrequencyPerDay = dto.FrequencyPerDay;
            assignment.Status = dto.Status;

            await _dbContext.SaveChangesAsync();

            return new AssignmentDto
            {
                ExerciseAssignmentId = assignment.ExerciseAssignmentId,
                PatientId = assignment.PatientId,
                ExerciseId = assignment.ExerciseId,
                ExerciseName = exerciseName,
                TherapistName = assignment.TherapistName,
                Sets = assignment.Sets,
                Reps = assignment.Reps,
                DurationMinutes = assignment.DurationMinutes,
                FrequencyPerWeek = assignment.FrequencyPerWeek,
                FrequencyPerDay = assignment.FrequencyPerDay,
                Status = assignment.Status,
                Feedback = assignment.Feedback,
                AssignedAt = assignment.AssignedAt,
                CompletedAt = assignment.CompletedAt
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var assignment = await _dbContext.ExerciseAssignments
                .FirstOrDefaultAsync(ea => ea.ExerciseAssignmentId == id);

            if (assignment == null) return false;

            assignment.IsDeleted = true;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
