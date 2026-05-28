using System.Text.Json.Serialization;

namespace PhysioLink.AdminPanel.Services
{
    public class PagedResult<T>
    {
        [JsonPropertyName("items")]
        public List<T> Items { get; set; } = [];

        [JsonPropertyName("totalRecordCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }
    }
}
