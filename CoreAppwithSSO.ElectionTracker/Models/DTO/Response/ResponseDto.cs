namespace CoreAppwithSSO.ElectionTracker.Models.DTO.Response
{
    public class ResponseDto
    {
        public bool IsError { get; set; } = false;
        public string ResponseCode { get; set; } = string.Empty;
        public object? Data { get; set; }
        public List<string> Exceptions { get; set; } = new List<string>();
    }
}
