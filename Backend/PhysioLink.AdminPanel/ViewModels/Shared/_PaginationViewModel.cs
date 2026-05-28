namespace PhysioLink.AdminPanel.ViewModels.Shared
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string ActionName { get; set; } = "Index";
        public Dictionary<string, string> Filters { get; set; } = new();
    }
}
