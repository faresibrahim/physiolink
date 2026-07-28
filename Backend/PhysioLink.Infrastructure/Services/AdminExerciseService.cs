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

        public AdminExerciseService(PhysioLinkDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<AdminExerciseDto>> GetAllAsync(string? search = null, string? difficulty = null, string? category = null)
        {
            bool filterByDifficulty = Enum.TryParse<DifficultyLevel>(difficulty, ignoreCase: true, out var parsedDifficulty);
            bool filterByCategory   = Enum.TryParse<ExerciseCategory>(category,  ignoreCase: true, out var parsedCategory);

            return await _dbContext.Exercises
                .AsNoTracking()
                .Where(e => !filterByDifficulty || e.Difficulty == parsedDifficulty)
                .Where(e => !filterByCategory   || e.Category   == parsedCategory)
                .Where(e => search == null || e.Name.ToLower().Contains(search.ToLower()))
                .OrderBy(e => e.Name)
                .Select(e => new AdminExerciseDto
                {
                    ExerciseId = e.ExerciseId,
                    Name = e.Name,
                    Sets = e.Sets,
                    Reps = e.Reps,
                    DurationMinutes = e.DurationMinutes,
                    Description = e.Description,
                    DescriptionAr = e.DescriptionAr,
                    VideoUrl = e.VideoUrl,
                    Difficulty = e.Difficulty,
                    Category = e.Category
                })
                .ToListAsync();
        }

        public async Task<AdminExerciseDto?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Exercises
                .AsNoTracking()
                .Where(e => e.ExerciseId == id)
                .Select(e => new AdminExerciseDto
                {
                    ExerciseId = e.ExerciseId,
                    Name = e.Name,
                    Sets = e.Sets,
                    Reps = e.Reps,
                    DurationMinutes = e.DurationMinutes,
                    Description = e.Description,
                    DescriptionAr = e.DescriptionAr,
                    VideoUrl = e.VideoUrl,
                    Difficulty = e.Difficulty,
                    Category = e.Category
                })
                .FirstOrDefaultAsync();
        }

        public async Task<AdminExerciseDto> CreateAsync(CreateAdminExerciseDto dto)
        {
            var exercise = new Exercise(
                dto.Name, dto.Reps, dto.Sets, dto.DurationMinutes,
                dto.Description, dto.Difficulty, dto.Category, dto.DescriptionAr);
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
                DescriptionAr = exercise.DescriptionAr,
                VideoUrl = exercise.VideoUrl,
                Difficulty = exercise.Difficulty,
                Category = exercise.Category
            };
        }
    }
}
