namespace PhysioLink.AdminPanel.ViewModels.Shared
{
    // Footer of a data-table card: "Showing X to Y of Z entries" + pagination.
    public class DataTableFooterViewModel
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int ItemCount { get; set; }
        public PaginationViewModel Pagination { get; set; } = new();
    }
}
