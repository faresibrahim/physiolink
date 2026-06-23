using PhysioLink.AdminPanel.Services;
using PhysioLink.AdminPanel.ViewModels.Shared;

namespace PhysioLink.AdminPanel.ViewModels
{
    public class ExerciseListViewModel
    {
        public List<ExerciseResponse> Exercises { get; set; } = [];
        public int TotalCount { get; set; }
        public string? SearchQuery { get; set; }
        public string? DifficultyFilter { get; set; }
        public string? CategoryFilter { get; set; }
        public PaginationViewModel Pagination { get; set; } = new();

    }

    public class ExerciseDetailViewModel
    {
        public Guid ExerciseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Sets { get; set; }
        public int Reps { get; set; }
        public int DurationMinutes { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public bool HasVideo => !string.IsNullOrWhiteSpace(VideoUrl);
    }
}
