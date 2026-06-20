namespace CoreAppwithSSO.ElectionTracker.Models
{
    public class ATMDto
    {
        public int GenericId { get; set; }
    }
    public class ATMErrorDto
    {
        public LogType LogType { get; set; }
        public string FunctionName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public string IPInformation { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public Exception AppException { get; set; } = new Exception();
    }
    public class ATMErrorExtensionDto
    {
        public ATMErrorDto Error { get; set; } = new ATMErrorDto();
        public string Message { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public string TargetSite { get; set; } = string.Empty;
    }

    public class CustomErrorDto
    {
        public LogType LogType { get; set; }
        public string FunctionName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public string IPInformation { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Targetsite { get; set; } = string.Empty;
        public string? JsonData { get; set; } = string.Empty;

    }

    public class CustomErrorExtensionDto
    {
        public CustomErrorDto Error { get; set; } = new CustomErrorDto();
        public DateTime LogDate { get; set; } = DateTime.MinValue;
        public string Message { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public string TargetSite { get; set; } = string.Empty;
        public string LogTable { get; set; } = string.Empty;
        public string? JsonData { get; set; } = string.Empty;
        public ProfileOptionRequest profileOption { get; set; } = new ProfileOptionRequest();
    }

    public class ProfileOptionRequest
    {
        public string ProfileOptionName { get; set; } = string.Empty;
        public int ProfileOptionId { get; set; }
        public int UserId { get; set; }
        public int OrganizationId { get; set; }
        public int BusinessUnitId { get; set; }
        public string ModuleCode { get; set; } = string.Empty;
    }
    public class ProfileOptionValues
    {
        public int ProfileOptionValueId { get; set; }
        public int LevelId { get; set; }
        public string LevelValue { get; set; } = string.Empty;
        public string ProfileOptionValue { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;

    }
    public class ProfileOptionValueResponse
    {
        public int ProfileOptionValueId { get; set; }
        public int LevelId { get; set; }
        public string LevelValue { get; set; } = string.Empty;
        public string ProfileOptionValue { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
    }
    public enum LogType
    {
        Error,
        Trace,
        Debug,
        Info,
        Warning,
        Fatal
    }
}
