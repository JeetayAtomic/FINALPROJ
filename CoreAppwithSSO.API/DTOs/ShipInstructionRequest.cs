namespace CoreAppwithSSO.API.DTOs
{
    public class ShipInstructionRequest
    {
        public string BillNumber { get; set; }
        public int Order_Id { get; set; }
        public List<int> Instruction_Ids { get; set; }
    }
}
