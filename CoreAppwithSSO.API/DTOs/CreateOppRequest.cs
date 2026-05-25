namespace CoreAppwithSSO.API.DTOs
{
    public class CreateOppRequest
    {
        public List<OppDetailsRequest> Details { get; set; }
        public OppMainRequest Main { get; set; }
        public ShipInstructionRequest ShipInstructions { get; set; }
    }
}
