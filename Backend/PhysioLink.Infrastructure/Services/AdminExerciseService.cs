using Microsoft.EntityFrameworkCore;
using PhysioLink.Application.DTOs.Exercises;
using PhysioLink.Application.Interfaces;
using PhysioLink.Domain.Entities;
using PhysioLink.Domain.Enums;
using PhysioLink.Infrastructure.Data;

namespace PhysioLink.Infrastructure.Services
{
    public class AdminExerciseService : IAdminExerciseService
    {
        private readonly PhysioLinkDbContext _dbContext;
        private readonly ICurrentClinicService _currentClinicService;

        public AdminExerciseService(PhysioLinkDbContext dbContext, ICurrentClinicService currentClinicService)
        {
            _dbContext = dbContext;
            _currentClinicService = currentClinicService;
        }

        public async Task<List<AdminExerciseDto>> GetAllAsync(string? search = null, string? difficulty = null)
        {
            var clinicId = _currentClinicService.GetCurrentClinicId()
                ?? throw new InvalidOperationException("No clinic in context.");

            bool filterByDifficulty = Enum.TryParse<DifficultyLevel>(difficulty, ignoreCase: true, out var parsedDifficulty);

            // Clinic-specific — manually filter by clinicId + IsDeleted (Exercise is AuditableEntity, no automatic clinic scoping)
            var clinicExercises = _dbContext.Exercises
                .AsNoTracking()
                .Where(e => e.ClinicId == clinicId && !e.IsDeleted)
                .Where(e => !filterByDifficulty || e.Difficulty == parsedDifficulty)
                .Where(e => search == null || e.Name.ToLower().Contains(search.ToLower()))
                .Select(e => new AdminExerciseDto
                {
                    ExerciseId = e.ExerciseId,
                    Name = e.Name,
                    Sets = e.Sets,
                    Reps = e.Reps,
                    DurationMinutes = e.DurationMinutes,
                    Description = e.Description,
                    VideoUrl = e.VideoUrl,
                    Difficulty = e.Difficulty,
                    IsGlobal = false
                });

            // Global exercises — bypass filter, manually enforce IsDeleted
            var globalExercises = _dbContext.Exercises
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(e => e.ClinicId == null && !e.IsDeleted)
                .Where(e => !filterByDifficulty || e.Difficulty == parsedDifficulty)
                .Where(e => search == null || e.Name.Contains(search))
                .Select(e => new AdminExerciseDto
                {
                    ExerciseId = e.ExerciseId,
                    Name = e.Name,
                    Sets = e.Sets,
                    Reps = e.Reps,
                    DurationMinutes = e.DurationMinutes,
                    Description = e.Description,
                    VideoUrl = e.VideoUrl,
                    Difficulty = e.Difficulty,
                    IsGlobal = true
                });

            return await clinicExercises.Concat(globalExercises)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<AdminExerciseDto> CreateAsync(CreateAdminExerciseDto dto)
        {
            var clinicId = _currentClinicService.GetCurrentClinicId()
                ?? throw new InvalidOperationException("No clinic in context.");

            var exercise = new Exercise(
                dto.Name,
                dto.Reps,
                dto.Sets,
                dto.DurationMinutes,
                dto.Description,
                dto.Difficulty
            );
            exercise.ClinicId = clinicId;
            exercise.VideoUrl = dto.VideoUrl;

            _dbContext.Exercises.Add(exercise);
            await _dbContext.SaveChangesAsync();

            return new AdminExerciseDto
            {
                ExerciseId = exercise.ExerciseId,
                Name = exercise.Name,
                Sets = exercise.Sets,
                Reps = exercise.Reps,
                DurationMinutes = exercise.DurationMinutes,
                Description = exercise.Description,
                VideoUrl = exercise.VideoUrl,
                Difficulty = exercise.Difficulty,
                IsGlobal = false
            };
        }
    }
}
