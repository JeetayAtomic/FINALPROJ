namespace CoreAppwithSSO.ElectionTracker.Models.DTO.Request
{
    public class SearchFilterRequest
    {
        public string SortColumn { get; set; } = string.Empty;
        public string SortOrder { get; set; } = string.Empty;
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Filters { get; set; } = string.Empty;
        public List<DynamicSearchParameters> DynamicSearchParameters { get; set; } = [];
    }
}
