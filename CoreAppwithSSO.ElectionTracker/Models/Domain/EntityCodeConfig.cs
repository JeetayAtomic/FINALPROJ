namespace CoreAppwithSSO.ElectionTracker.Models.Domain
{
    public class EntityCodeConfig
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = default!;
        public string Prefix { get; set; } = default!;
        public string? Postfix { get; set; }
        public int StartValue { get; set; }
        public int NextSeq { get; set; }
        public int PadLength { get; set; }
        public bool IsActive { get; set; }
    }
}
