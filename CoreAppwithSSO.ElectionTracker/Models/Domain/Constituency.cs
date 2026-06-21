namespace CoreAppwithSSO.ElectionTracker.Models.Domain
{
    public class Constituency
    {
        public int Id { get; set; }
        public int DistrictId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "AC";
        public Int32 StateId { get; set; } = 0;
        public string Region { get; set; } = string.Empty;
        public string SubRegion { get; set; } = string.Empty;
    }
}
