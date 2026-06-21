namespace CoreAppwithSSO.ElectionTracker.Models.Domain
{
    public class BoothMaterial
    {
        public int BoothMaterialId { get; set; } = 0;
        public string Evm { get; set; } = "";
        public string VoterRoll { get; set; } = "";
        public string InkPad { get; set; } = "";
        public string Stationery { get; set; } = "";
        public string Signage { get; set; } = "";
    }
}
