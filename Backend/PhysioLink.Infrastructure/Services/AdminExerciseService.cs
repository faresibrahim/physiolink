using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;

        // The exercise catalog is a global (non-clinic-scoped) reference list that
        // changes only when an admin adds an exercise — rare. So we cache the whole
        // list under one key and filter in memory, and rely on explicit eviction
        // (see CreateAsync) for correctness. The long TTL is only a backstop in case
        // a future write path forgets to evict.
        private const string AllExercisesCacheKey = "exercises:all";
        private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(6);

        public AdminExerciseService(PhysioLinkDbContext dbContext, IMemoryCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        public async Task<List<AdminExerciseDto>> GetAllAsync(string? search = null, string? difficulty = null, string? category = null)
        {
            bool filterByDifficulty = Enum.TryParse<DifficultyLevel>(difficulty, ignoreCase: true, out var parsedDifficulty);
            bool filterByCategory   = Enum.TryParse<ExerciseCategory>(category,  ignoreCase: true, out var parsedCategory);

            // Cache the full, unfiltered catalog once; serve every filter combination
            // from it in memory. This keeps the cache to a single entry instead of one
            // per (search, difficulty, category) permutation.
            var all = await GetAllCachedAsync();

            IEnumerable<AdminExerciseDto> result = all;
            if (filterByDifficulty)
                result = result.Where(e => e.Difficulty == parsedDifficulty);
            if (filterByCategory)
                result = result.Where(e => e.Category == parsedCategory);
            if (search != null)
                result = result.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

            // ToList() hands callers a fresh list — never the cached instance itself.
            return result.ToList();
        }

        private async Task<List<AdminExerciseDto>> GetAllCachedAsync()
        {
            return await _cache.GetOrCreateAsync(AllExercisesCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;
                return await _dbContext.Exercises
                    .AsNoTracking()
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
            }) ?? new List<AdminExerciseDto>();
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

            // Explicit eviction: the catalog just changed, so drop the cached list.
            // The next GetAllAsync rebuilds it from the database. Any future write path
            // (update / delete) MUST evict this key too, or it will serve stale data.
            _cache.Remove(AllExercisesCacheKey);

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
