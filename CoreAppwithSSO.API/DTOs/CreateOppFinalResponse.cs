namespace CoreAppwithSSO.API.DTOs
{
    public class CreateOppFinalResponse
    {
        public string? BillNumber { get; set; }
        public int? OrderId { get; set; }

        public OppDetailsResponse? Details { get; set; }
        public OppMainResponse? Main { get; set; }
        public ShipInstructionResponse? ShipInstructions { get; set; }

        public bool IsSuccess { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class OppDetailsResponse
    {
        public string? BillNumber { get; set; }
        public int? OrderId { get; set; }
        public string? TempControlled { get; set; }
        public int? LowTemp { get; set; }
        public int? HighTemp { get; set; }
        public string? TempUnits { get; set; }
        public string? DangerousGoods { get; set; }
        public int? CubeEst { get; set; }
        public List<DetailItem>? DtlJsonArray { get; set; }
        public ResultInfo ? Result { get; set; }
    }

    public class OppMainResponse
    {
        public string? Origin { get; set; }
        public string? OrigName { get; set; }
        public string? OrigAddr1 { get; set; }
        public string? OrigAddr2 { get; set; }
        public string? OrigCity { get; set; }
        public string? OrigProv { get; set; }
        public string? OrigPc { get; set; }
        public string? OrigContact { get; set; }
        public string? OrigPhone { get; set; }
        public string? OrigPhoneExt { get; set; }
        public string? OrigCountry { get; set; }
        public string? StartZone { get; set; }

        public string? Destination { get; set; }
        public string? DestName { get; set; }
        public string? DestAddr1 { get; set; }
        public string? DestAddr2 { get; set; }
        public string? DestCity { get; set; }
        public string? DestProv { get; set; }
        public string? DestPc { get; set; }
        public string? DestCountry { get; set; }
        public string? DestContact { get; set; }
        public string? DestPhone { get; set; }
        public string? DestPhoneExt { get; set; }
        public string? EndZone { get; set; }

        public string? Customer { get; set; }
        public string? CallName { get; set; }
        public string? CallAddr1 { get; set; }
        public string? CallAddr2 { get; set; }
        public string? CallCity { get; set; }
        public string? CallProv { get; set; }
        public string? CallPc { get; set; }
        public string? CallCountry { get; set; }
        public string? CallContact { get; set; }
        public string? CallPhone { get; set; }
        public string? CallPhoneExt { get; set; }

        public string? PickupBy { get; set; }
        public string? PickupByEnd { get; set; }
        public string? PickupApptReq { get; set; }

        public string? DeliverBy { get; set; }
        public string? DeliverByEnd { get; set; }
        public string? DeliverApptReq { get; set; }

        public string? CurrentStatus { get; set; }
        public string? OrderId { get; set; }
        public string? BillNumber { get; set; }

        public string? ServiceLevel { get; set; }
        public string? TempControlled { get; set; }
        public string? Temperature { get; set; }

        public string? OpCode { get; set; }
        public string? SiteId { get; set; }

        public string? BillToCode { get; set; }
        public string? BillToName { get; set; }

        public string? ExtraStops { get; set; }
        public string? WebStatus { get; set; }
        public string? BillTo { get; set; }
        public string? CurrencyCode { get; set; }

        public string? Posted { get; set; }
        public string? CreatedBy { get; set; }
        public string? WebUsername { get; set; }
        public string? CreatedTime { get; set; }

        public ResultInfo? Result { get; set; }
    }
    public class ShipInstructionResponse
    {
        public string? BillNumber { get; set; }
        public int? Order_Id { get; set; }
        public List<int>? Instruction_Ids { get; set; }
        public ResultInfo? Result { get; set; }
    }
    public class ResultInfo
    {
        public bool? Success { get; set; }
        public int? ResultValue { get; set; }
        public string? ErrorMessage { get; set; }
        public bool? HasError { get; set; }
    }
}
