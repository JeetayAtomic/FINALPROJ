namespace CoreAppwithSSO.ElectionTracker.Models.Domain
{
    public class BoothPerson
    {
        public Int32 BoothPersonId { get; set; }
        public Int32 BoothId { get; set; } = 0;
        public string Name { get; set; } = "";
        public string Initials { get; set; } = "";
    }
}
