namespace ATM.Core.ISC.Models;

public class ATMResponse<T>
{
    public Boolean IsError { get; set; } = false;
    public string ResponseCode { get; set; } = string.Empty;
    public List<Dictionary<string, string>> Tokens { get; set; } = new List<Dictionary<string, string>>();
    public List<T> Data { get; set; } = new List<T>();
    public List<ErrorDescription> Errors { get; set; } = new List<ErrorDescription>();
    public List<string> Exceptions { get; set; } = new List<string>();
    public string ResponseMessageType { get; set; } = string.Empty;
}

public class ErrorDescription
{
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
}
