namespace CoreAppwithSSO.ElectionTracker.Extension
{
    public class BadRequestException(string errorCode, string? errorMessage = null) : Exception(errorMessage ?? "Business validation failed")
    {
        public string ErrorCode { get; } = errorCode;
    }
}
