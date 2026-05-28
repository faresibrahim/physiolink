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

        public ExercisesController (ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? difficulty, int page = 1)
        {
            var exercises = await _apiClient.GetExercisesAsync(search, difficulty);
            if(exercises == null)
            {
                return SessionExpiredRedirect();
            }

            var filters = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(search))
                filters["searchQuery"] = search;
            

            var viewModel = new ExerciseListViewModel
            {
                Exercises = exercises,
                SearchQuery = search,
                DifficultyFilter = difficulty,
                Pagination = new PaginationViewModel
                {
                    CurrentPage = page,
                    TotalPages = 1,

                }
            };
            return View(viewModel);
        }
    }
}
