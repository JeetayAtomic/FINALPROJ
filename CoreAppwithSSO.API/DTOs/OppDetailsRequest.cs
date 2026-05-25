namespace CoreAppwithSSO.API.DTOs
{
    public class OppDetailsRequest
    {
        public string BillNumber { get; set; }
        public string TempControlled { get; set; }
        public decimal LowTemp { get; set; }
        public decimal HighTemp { get; set; }
        public string TempUnits { get; set; }
        public string DangerousGoods { get; set; }
        public decimal CubeEst { get; set; }
        public List<DetailItem> DtlJsonArray { get; set; }
    }

    public class DetailItem
    {
        public int? Sequence { get; set; }
        public int? SortOrder { get; set; }
        public string Commodity { get; set; }
        public string? Labels { get; set; }
        public string Pallets { get; set; }
        public string? Pieces { get; set; }
        public string Weight { get; set; }
        public string PalletUnits { get; set; }
        public string PiecesUnits { get; set; }
        public string WeightUnits { get; set; }
        public string Description { get; set; }
    }
}
