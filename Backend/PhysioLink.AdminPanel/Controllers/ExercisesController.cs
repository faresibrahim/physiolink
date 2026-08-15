using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhysioLink.AdminPanel.Services;
using PhysioLink.AdminPanel.ViewModels;
using PhysioLink.AdminPanel.ViewModels.Shared;

namespace PhysioLink.AdminPanel.Controllers
{
    [Authorize]
    public class ExercisesController : BaseController
    {
        private readonly ApiClient _apiClient;
        private readonly IConfiguration _configuration;

        public ExercisesController (ApiClient apiClient, IConfiguration configuration)
        {
            _apiClient = apiClient;
            _configuration = configuration;
        }

        private const int PageSize = 12;

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? difficulty, string? category, int page = 1)
        {
            var exercises = await _apiClient.GetExercisesAsync(search, difficulty, category);
            if(exercises == null)
            {
                return SessionExpiredRedirect();
            }

            // Client-side pagination — the exercises API returns a flat list, so we
            // slice here. The Pagination partial then renders identically to other pages.
            var totalCount = exercises.Count;
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var pageItems = exercises
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var filters = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(search))     filters["search"]     = search;
            if (!string.IsNullOrWhiteSpace(difficulty)) filters["difficulty"] = difficulty;
            if (!string.IsNullOrWhiteSpace(category))   filters["category"]   = category;

            var viewModel = new ExerciseListViewModel
            {
                Exercises = pageItems,
                TotalCount = totalCount,
                SearchQuery = search,
                DifficultyFilter = difficulty,
                CategoryFilter = category,
                Pagination = new PaginationViewModel
                {
                    CurrentPage = page,
                    TotalPages = totalPages,
                    ActionName = "Index",
                    Filters = filters,
                }
            };
            return View(viewModel);
        }

        // JSON proxy for the detail modal — runs server-side so the API call uses
        // the panel's HttpOnly auth cookie via ApiClient (no token exposed to JS).
        [HttpGet]
        public async Task<IActionResult> GetJson(Guid id)
        {
            var exercise = await _apiClient.GetExerciseByIdAsync(id);
            if (exercise is null)
            {
                return NotFound();
            }

            return Ok(new
            {
                exerciseId      = exercise.ExerciseId,
                name            = exercise.Name,
                description     = exercise.Description,
                descriptionAr   = exercise.DescriptionAr,
                sets            = exercise.Sets,
                reps            = exercise.Reps,
                durationMinutes = exercise.DurationMinutes,
                difficulty      = exercise.Difficulty,
                category        = exercise.Category,
                videoUrl        = ResolveVideoUrl(exercise.VideoUrl)
            });
        }

        // Server-relative video paths (e.g. /videos/xxx.mp4) are stored relative to
        // the API host, not this admin panel — resolve them to an absolute URL so
        // the browser fetches the file from the right origin. Full URLs (YouTube,
        // or any other already-absolute link) pass through unchanged.
        //
        // The resolved URL is embedded in the page and fetched by the USER'S BROWSER,
        // so it must point at the API's PUBLIC address. ApiBaseUrl may be a private
        // internal address (e.g. Railway's *.railway.internal host) that the panel
        // uses for fast server-to-server calls but the browser cannot reach — using
        // it here loads the modal data yet leaves the video broken. Prefer the
        // public media base and fall back to ApiBaseUrl for local dev, where they
        // are the same localhost origin.
        private string? ResolveVideoUrl(string? videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl)) return videoUrl;
            if (videoUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return videoUrl;

            var mediaBaseUrl = (_configuration["PublicApiBaseUrl"] ?? _configuration["ApiBaseUrl"])!.TrimEnd('/');
            return $"{mediaBaseUrl}{videoUrl}";
        }
    }
}
