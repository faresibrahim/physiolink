using PhysioLink.AdminPanel.Services;
using PhysioLink.AdminPanel.ViewModels.Shared;

namespace PhysioLink.AdminPanel.ViewModels
{
    public class ExerciseListViewModel
    {
        public List<ExerciseResponse> Exercises { get; set; } = [];
        public string? SearchQuery { get; set; }
        public string? DifficultyFilter { get; set; }
        public PaginationViewModel Pagination { get; set; } = new();

    }
}
