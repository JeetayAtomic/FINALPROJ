namespace CoreAppwithSSO.ElectionTracker.Common
{
    public class ATMResponse<T>
    {
        public bool IsError { get; set; } = false;
        public string ResponseCode { get; set; } = string.Empty;
        public List<Dictionary<string, string>> Tokens { get; set; } = [];
        public List<T> Data { get; set; } = [];
        public List<ErrorDescription> Errors { get; set; } = [];
        public List<string> Exceptions { get; set; } = [];
        public string ResponseMessageType { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }

    public class ErrorDescription
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
    }
}
