using System.Text.Json.Serialization;

namespace CoreAppwithSSO.API.DTOs.Client
{
    public class OrderInfoResponse
    {
        [JsonPropertyName("detail_line_id")]
        public long DetailLineId { get; set; }

        [JsonPropertyName("bill_number")]
        public string? BillNumber { get; set; }

        [JsonPropertyName("customer")]
        public string? Customer { get; set; }

        [JsonPropertyName("callname")]
        public string? CallName { get; set; }

        [JsonPropertyName("calladdr1")]
        public string? CallAddr1 { get; set; }

        [JsonPropertyName("calladdr2")]
        public string? CallAddr2 { get; set; }

        [JsonPropertyName("callcity")]
        public string? CallCity { get; set; }

        [JsonPropertyName("callprov")]
        public string? CallProv { get; set; }

        [JsonPropertyName("callpc")]
        public string? CallPostalCode { get; set; }

        [JsonPropertyName("callphone")]
        public string? CallPhone { get; set; }

        [JsonPropertyName("callphoneext")]
        public string? CallPhoneExt { get; set; }

        [JsonPropertyName("callfax")]
        public string? CallFax { get; set; }

        [JsonPropertyName("callcell")]
        public string? CallCell { get; set; }

        [JsonPropertyName("callcontact")]
        public string? CallContact { get; set; }

        [JsonPropertyName("callemail")]
        public string? CallEmail { get; set; }

        [JsonPropertyName("origin")]
        public string? OriginCode { get; set; }

        [JsonPropertyName("origname")]
        public string? OriginName { get; set; }

        [JsonPropertyName("origaddr1")]
        public string? OriginAddr1 { get; set; }

        [JsonPropertyName("origaddr2")]
        public string? OriginAddr2 { get; set; }

        [JsonPropertyName("origcity")]
        public string? OriginCity { get; set; }

        [JsonPropertyName("origprov")]
        public string? OriginProv { get; set; }

        [JsonPropertyName("origpc")]
        public string? OriginPostalCode { get; set; }

        [JsonPropertyName("origphone")]
        public string? OriginPhone { get; set; }

        [JsonPropertyName("origphoneext")]
        public string? OriginPhoneExt { get; set; }

        [JsonPropertyName("origfax")]
        public string? OriginFax { get; set; }

        [JsonPropertyName("origcell")]
        public string? OriginCell { get; set; }

        [JsonPropertyName("origcontact")]
        public string? OriginContact { get; set; }

        [JsonPropertyName("origemail")]
        public string? OriginEmail { get; set; }

        [JsonPropertyName("destination")]
        public string? DestinationCode { get; set; }

        [JsonPropertyName("destname")]
        public string? DestinationName { get; set; }

        [JsonPropertyName("destaddr1")]
        public string? DestinationAddr1 { get; set; }

        [JsonPropertyName("destaddr2")]
        public string? DestinationAddr2 { get; set; }

        [JsonPropertyName("destcity")]
        public string? DestinationCity { get; set; }

        [JsonPropertyName("destprov")]
        public string? DestinationProv { get; set; }

        [JsonPropertyName("destpc")]
        public string? DestinationPostalCode { get; set; }

        [JsonPropertyName("destphone")]
        public string? DestinationPhone { get; set; }

        [JsonPropertyName("destphoneext")]
        public string? DestinationPhoneExt { get; set; }

        [JsonPropertyName("destfax")]
        public string? DestinationFax { get; set; }

        [JsonPropertyName("destcell")]
        public string? DestinationCell { get; set; }

        [JsonPropertyName("destcontact")]
        public string? DestinationContact { get; set; }

        [JsonPropertyName("destemail")]
        public string? DestinationEmail { get; set; }

        [JsonPropertyName("created_time")]
        public DateTime CreatedTime { get; set; }

        [JsonPropertyName("created_by")]
        public string? CreatedBy { get; set; }

        [JsonPropertyName("current_zone")]
        public string? CurrentZone { get; set; }

        [JsonPropertyName("charges")]
        public decimal Charges { get; set; }

        [JsonPropertyName("xcharges")]
        public decimal ExtraCharges { get; set; }

        [JsonPropertyName("current_status")]
        public string? CurrentStatus { get; set; }

        [JsonPropertyName("pallets")]
        public decimal Pallets { get; set; }

        [JsonPropertyName("pieces")]
        public int Pieces { get; set; }

        [JsonPropertyName("length_1")]
        public decimal Length { get; set; }

        [JsonPropertyName("cube")]
        public decimal Cube { get; set; }

        [JsonPropertyName("weight")]
        public decimal Weight { get; set; }

        [JsonPropertyName("pick_up_by")]
        public DateTime? PickUpBy { get; set; }

        [JsonPropertyName("deliver_by")]
        public DateTime? DeliverBy { get; set; }

        [JsonPropertyName("dangerous_goods")]
        public string? DangerousGoods { get; set; }

        [JsonPropertyName("start_zone")]
        public string? StartZone { get; set; }

        [JsonPropertyName("end_zone")]
        public string? EndZone { get; set; }

        [JsonPropertyName("commodity")]
        public string? Commodity { get; set; }

        [JsonPropertyName("trace_no")]
        public string? TraceNumber { get; set; }

        [JsonPropertyName("service_level")]
        public string? ServiceLevel { get; set; }

        [JsonPropertyName("total_charges")]
        public decimal TotalCharges { get; set; }

        [JsonPropertyName("tax_1")]
        public decimal Tax1 { get; set; }

        [JsonPropertyName("tax_2")]
        public decimal Tax2 { get; set; }

        [JsonPropertyName("bill_date")]
        public DateTime BillDate { get; set; }

        [JsonPropertyName("temp_controlled")]
        public string? TempControlled { get; set; }

        [JsonPropertyName("master_order")]
        public int MasterOrder { get; set; }

        [JsonPropertyName("site_id")]
        public string? SiteId { get; set; }

        [JsonPropertyName("currency_code")]
        public string? CurrencyCode { get; set; }

        [JsonPropertyName("actual_pickup")]
        public DateTime? ActualPickup { get; set; }

        [JsonPropertyName("actual_delivery")]
        public DateTime? ActualDelivery { get; set; }

        [JsonPropertyName("temperature")]
        public decimal Temperature { get; set; }

        [JsonPropertyName("pickup_terminal")]
        public string? PickupTerminal { get; set; }

        [JsonPropertyName("delivery_terminal")]
        public string? DeliveryTerminal { get; set; }

        [JsonPropertyName("fsc_charges")]
        public decimal FscCharges { get; set; }

        [JsonPropertyName("ins_timestamp")]
        public DateTime InsTimestamp { get; set; }

        [JsonPropertyName("jarray_ship_instructions")]
        public List<ShipInstruction> ? ShipInstructions { get; set; }

        [JsonPropertyName("jarray_details")]
        public List<ShipmentDetail> ? Details { get; set; }

        [JsonPropertyName("jarray_traces")]
        public List<ShipmentTrace> ? Traces { get; set; }
    }
    public class ShipInstruction
    {
        [JsonPropertyName("instruction_id")]
        public int InstructionId { get; set; }

        [JsonPropertyName("short_desc")]
        public string? ShortDescription { get; set; }

        [JsonPropertyName("instruction")]
        public string? Instruction { get; set; }
    }
    public class ShipmentDetail
    {
        [JsonPropertyName("sequence")]
        public long Sequence { get; set; }

        [JsonPropertyName("commodity")]
        public string? Commodity { get; set; }

        [JsonPropertyName("pallets")]
        public decimal Pallets { get; set; }

        [JsonPropertyName("pieces")]
        public int Pieces { get; set; }

        [JsonPropertyName("length_1")]
        public decimal Length { get; set; }

        [JsonPropertyName("length_units")]
        public string? LengthUnits { get; set; }

        [JsonPropertyName("cube")]
        public decimal Cube { get; set; }

        [JsonPropertyName("cube_units")]
        public string? CubeUnits { get; set; }

        [JsonPropertyName("weight")]
        public decimal Weight { get; set; }

        [JsonPropertyName("weight_units")]
        public string? WeightUnits { get; set; }

        [JsonPropertyName("dangerous_goods")]
        public string? DangerousGoods { get; set; }

        [JsonPropertyName("pieces_units")]
        public string? PiecesUnits { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("temperature")]
        public decimal Temperature { get; set; }

        [JsonPropertyName("items")]
        public int Items { get; set; }

        [JsonPropertyName("pallet_units")]
        public string? PalletUnits { get; set; }

        [JsonPropertyName("density")]
        public decimal Density { get; set; }

        [JsonPropertyName("perishable")]
        public string? Perishable { get; set; }
    }
    public class ShipmentTrace
    {
        [JsonPropertyName("trace_number")]
        public string? TraceNumber { get; set; }

        [JsonPropertyName("trace_type")]
        public string? TraceType { get; set; }

        [JsonPropertyName("ref_qualifier")]
        public string? RefQualifier { get; set; }

        [JsonPropertyName("trace_desc")]
        public string? TraceDescription { get; set; }
    }
}
