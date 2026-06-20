namespace CoreAppwithSSO.ElectionTracker.Models.DTO.Response
{
    public interface ISearchResult<T>
    {
        /// <summary>
        /// Total Record
        /// </summary>
        int TotalRecord { get; set; }

        /// <summary>
        /// Results
        /// </summary>
        List<T> Results { get; set; }
    }
}
