namespace CoreAppwithSSO.ElectionTracker.Models.Domain
{
    public class Booth : BaseModel
    {
        public int BoothId { get; set; }

        // Business identity (originally "ShortId" on the client model).
        public string BoothCode { get; set; } = string.Empty;

        public int WardId { get; set; }
        public string BoothName { get; set; } = string.Empty;
        public string BoothNeighborhood { get; set; } = string.Empty;
        public Int32 ConstituencyId { get; set; } = 0;
        public string Venue { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string AddressVillage { get; set; } = string.Empty;
        public string AddressStreet { get; set; } = string.Empty;

        public int Accessibility { get; set; }
        public int Status { get; set; } = 0;
        public string StatusColor { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
