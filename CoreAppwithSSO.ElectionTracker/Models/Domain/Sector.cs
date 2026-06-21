namespace CoreAppwithSSO.ElectionTracker.Models.Domain
{
    public class Sector : BaseModel
    {
        public Int32 SectorId { get; set; }
        public Int32 BoothId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
