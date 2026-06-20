using System.Text.Json.Serialization;
using static CoreAppwithSSO.ElectionTracker.Common.CommonUtl;

namespace CoreAppwithSSO.ElectionTracker.Models.Domain
{
    public interface IAuditAttributes
    {
        public int CreatedBy { get; set; }
        public int LastUpdatedBy { get; set; }
    }
    public interface IAuditAttributesWithNames
    {
        public string CreatedByName { get; set; }
        public string LastUpdatedByName { get; set; }
    }
    public class BaseModel : IAuditAttributes
    {
        public string C_Attribute1 { get; set; } = string.Empty;
        public string C_Attribute2 { get; set; } = string.Empty;
        public string C_Attribute3 { get; set; } = string.Empty;
        public string C_Attribute4 { get; set; } = string.Empty;
        public decimal N_Attribute5 { get; set; } = 0;
        public decimal N_Attribute6 { get; set; } = 0;
        public decimal N_Attribute7 { get; set; } = 0;
        public decimal N_Attribute8 { get; set; } = 0;
       

        // Default to the current date/time (ISO-style, one of the accepted FlexDateFormats)
        // so callers that omit these still get a valid value instead of null.
        [DateTimeParametersAttribute]
        public string? D_Attribute9 { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        [DateTimeParametersAttribute]
        public string? D_Attribute10 { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        [DateTimeParametersAttribute]
        public string? D_Attribute11 { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        [DateTimeParametersAttribute]
        public string? D_Attribute12 { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        [DateTimeParametersAttribute]
        public string? D_Attribute13 { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string J_Attribute14 { get; set; } = string.Empty;
        public string J_Attribute15 { get; set; } = string.Empty;
        public string Record_Info { get; set; } = string.Empty;

        [JsonIgnore]
        public int CreatedBy { get; set; }
        [JsonIgnore]
        public int LastUpdatedBy { get; set; }

    }
}
