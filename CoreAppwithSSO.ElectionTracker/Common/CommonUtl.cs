using CoreAppwithSSO.ElectionTracker.Data.Sql;
using CoreAppwithSSO.ElectionTracker.Extension;
using CoreAppwithSSO.ElectionTracker.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CoreAppwithSSO.ElectionTracker.Common
{
    public static class CommonUtl
    {
        public static void AddParametersFromModel<T>(DynamicParameters parameters, T model, string InputOutput, List<string>? excludeProperties = null)
        {
            if (model != null)
            {
                excludeProperties ??= [];
                PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    if (excludeProperties.Contains(property.Name) || ExcludeParameter(property))
                    {
                        continue;
                    }
                    var value = property.GetValue(model);
                    if (value != null)
                    {
                        if (IsDateTimeColumn(property))
                        {
                            DateTime? validDate = ParseFlexDate(Convert.ToString(value), property.Name);
                            parameters.Add(property.Name, validDate);
                        }
                        else if (InputOutput == string.Empty)
                            parameters.Add(property.Name, value);
                        else
                            parameters.Add(property.Name, value, GetDbType(property.GetType()), ParameterDirection.InputOutput);
                    }
                }
            }
        }
        /// <summary>
        /// Builds an INSERT statement for <paramref name="tableName"/> covering every writable
        /// property on the model (including all BaseModel columns) and populates
        /// <paramref name="parameters"/> with the matching values. The supplied
        /// <paramref name="dialect"/> emits the engine-specific INSERT (returning the persisted
        /// row so the caller can hydrate the response). Pass identity/computed columns
        /// (e.g. the primary key) via <paramref name="excludeProperties"/>.
        /// </summary>
        public static string BuildInsertQuery<T>(DynamicParameters parameters, T model, string tableName, ISqlDialect dialect, List<string>? excludeProperties = null)
        {
            var columns = CollectColumns(parameters, model, excludeProperties);
            return dialect.BuildInsertReturning(tableName, columns);
        }

        /// <summary>
        /// Builds an UPDATE statement for <paramref name="tableName"/> that SETs every writable
        /// property (including all BaseModel columns) except <paramref name="keyColumn"/> and any
        /// in <paramref name="excludeProperties"/>, filtered by the key in the WHERE clause.
        /// The key is still added as a parameter for the WHERE clause. The supplied
        /// <paramref name="dialect"/> emits the engine-specific UPDATE returning the updated row.
        /// </summary>
        public static string BuildUpdateQuery<T>(DynamicParameters parameters, T model, string tableName, string keyColumn, ISqlDialect dialect, List<string>? excludeProperties = null)
        {
            var columns = CollectColumns(parameters, model, excludeProperties, keyColumn);
            return dialect.BuildUpdateReturning(tableName, columns, keyColumn);
        }

        /// <summary>
        /// Adds a parameter for every writable property on the model (honouring the same
        /// exclude/date-parsing rules as <see cref="AddParametersFromModel"/>) and returns the
        /// column names to use in the SQL. When <paramref name="keyColumn"/> is supplied its
        /// parameter is still added (for a WHERE clause) but the column is omitted from the result
        /// so it is not written in a SET/VALUES list.
        /// </summary>
        private static List<string> CollectColumns<T>(DynamicParameters parameters, T model, List<string>? excludeProperties, string? keyColumn = null)
        {
            var columns = new List<string>();
            if (model == null)
            {
                return columns;
            }

            excludeProperties ??= [];
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (excludeProperties.Contains(property.Name) || ExcludeParameter(property))
                {
                    continue;
                }

                object? value = property.GetValue(model);
                if (IsDateTimeColumn(property))
                {
                    value = ParseFlexDate(Convert.ToString(value), property.Name);
                }

                parameters.Add(property.Name, value);

                if (!string.Equals(property.Name, keyColumn, StringComparison.OrdinalIgnoreCase))
                {
                    columns.Add(property.Name);
                }
            }

            return columns;
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class ExcludeParametersAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Property)]
        public class DateTimeParametersAttribute : Attribute
        {
        }

        private static bool IsDateTimeColumn(PropertyInfo property)
        {
            if (property.GetCustomAttribute<DateTimeParametersAttribute>() != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Accepted date/time formats for flexfield columns. ISO 8601 first so behaviour
        /// is identical on every machine regardless of the server's culture.
        /// </summary>
        private static readonly string[] FlexDateFormats =
        {
            "yyyy-MM-ddTHH:mm:ss.fffffffK",
            "yyyy-MM-ddTHH:mm:ssK",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd"
        };

        /// <summary>
        /// Parses a flexfield date string in a culture-invariant way. Tries the explicit
        /// ISO 8601 formats first, then falls back to a general invariant-culture parse.
        /// Returns null only when the value is genuinely unparseable (not a locale mismatch).
        /// </summary>
        private static DateTime? ParseFlexDate(string? value, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (DateTime.TryParseExact(value, FlexDateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var exact))
            {
                return exact;
            }

            // Fallback: still culture-invariant so the result does not depend on server locale.
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsed))
            {
                return parsed;
            }

            throw new BadRequestException(
                "INVALID_DATE_FORMAT",
                $"Invalid date value '{value}' for '{propertyName}'. Expected ISO 8601 format (yyyy-MM-dd or yyyy-MM-ddTHH:mm:ss).");
        }
        private static bool ExcludeParameter(PropertyInfo property)
        {

            if (property.GetCustomAttribute<ExcludeParametersAttribute>() != null)
            {
                return true;
            }

            return false;
        }
        public static DbType GetDbType(Type runtimeType)
        {
            var nonNullableType = Nullable.GetUnderlyingType(runtimeType);
            if (nonNullableType != null)
            {
                runtimeType = nonNullableType;
            }

            var templateValue = null as object;
            if (runtimeType.IsClass == false)
            {
                templateValue = Activator.CreateInstance(runtimeType);
            }

            var sqlParamter = new SqlParameter(parameterName: string.Empty, value: templateValue);

            return sqlParamter.DbType;
        }

        // These helpers read from the framework-validated ClaimsPrincipal (HttpContext.User),
        // which the JwtBearer middleware populates only after fully validating the token.
        // The old implementations re-parsed the raw Authorization header with no signature
        // check — never do that.
        private static string? GetValidatedClaim(HttpContext httpContext, string claimType) =>
            httpContext.User?.FindFirst(claimType)?.Value;

        public static int GetUserIdFromHeader(HttpContext httpContext) =>
            int.TryParse(GetValidatedClaim(httpContext, "UserId"), out var userId) ? userId : 0;

        public static string GetEmailIDFromHeader(HttpContext httpContext) =>
            GetValidatedClaim(httpContext, "EmailID") ?? string.Empty;

        public static string GetUserNameFromHeader(HttpContext httpContext) =>
            GetValidatedClaim(httpContext, "UserName") ?? string.Empty;

        public static int GetLanguageIdFromHeader(HttpContext httpContext) =>
            int.TryParse(GetValidatedClaim(httpContext, "LanguageId"), out var langId) && langId > 0 ? langId : 1;

        public static int GetOrganizationIdFromHeader(HttpContext httpContext) =>
            int.TryParse(GetValidatedClaim(httpContext, "OrganizationId"), out var orgId) ? orgId : 0;
        public static string GetIPAddress(HttpContext httpContext)
        {
            if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                if (string.IsNullOrEmpty(httpContext.Request.Headers["X-Forwarded-For"].ToString()))
                    return string.Empty;
                else
                    return httpContext.Request.Headers["X-Forwarded-For"].ToString();
            }
            else if (httpContext.Connection.RemoteIpAddress != null)
            {
                return httpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            else { return string.Empty; }
        }
        public static bool ValidateEmailAddress(string sEmail)
        {
            string pattern = @"^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})$";
            Match match = Regex.Match(sEmail.Trim(), pattern, RegexOptions.IgnoreCase);
            if (match.Success)
                return true;
            else
                return false;
        }
        public static bool ValidateEmailAddress(string sEmail, int minLen, int maxLen)
        {
            if (sEmail == null) return false;
            if (minLen == 0) return false;
            if (maxLen == 0) return false;
            if (sEmail.Trim().Length < minLen || sEmail.Trim().Length > maxLen) return false;

            string pattern = @"^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})$";
            Match match = Regex.Match(sEmail.Trim(), pattern, RegexOptions.IgnoreCase);
            if (match.Success)
                return true;
            else
                return false;
        }
        public static string RemoveSpecialChars(string input)
        {
            if (input == null)
            {
                return "";
            }

            string[] chars = new string[] { ",", ".", "/", "!", "@", "#", "$", "%", "^", "&", "*", "'", "\"", ";", "-", "_", "(", ")", ":", "|", "[", "]" };
            for (int i = 0; i < chars.Length; i++)
            {
                if (input.Contains(chars[i]))
                {
                    input = input.Replace(chars[i], "");
                }
            }
            return input;
        }
        public static void ExtractConditions(JArray conditionArray, List<DynamicSearchParameters> searchParameters)
        {
            foreach (var item in conditionArray)
            {
                if (item is JArray innerArray)
                {
                    // If it contains exactly 3 items, it's a valid condition
                    if (innerArray.Count == 3 && innerArray[0] is JValue && innerArray[1] is JValue && innerArray[2] is JValue)
                    {
                        searchParameters.Add(new DynamicSearchParameters
                        {
                            DynamicColumnName = innerArray[0].ToString(),
                            DynamicColumnOprator = innerArray[1].ToString(),
                            DynamicColumnValue = innerArray[2].ToString()
                        });
                    }
                    else
                    {
                        // Recursively process nested arrays
                        ExtractConditions(innerArray, searchParameters);
                    }
                }
            }
        }

        public static void SetDynamicParametersData(List<DynamicSearchParameters> DynamicSearchParameters, out string dynamicSearchParameters)
        {
            XElement xDynamicSearchParameters = new("xDynamicSearchParameters");
            dynamicSearchParameters = string.Empty;
            foreach (var _dynamicSearchParameters in DynamicSearchParameters)
            {
                if (!string.IsNullOrEmpty(_dynamicSearchParameters.DynamicColumnName) && !string.IsNullOrEmpty(_dynamicSearchParameters.DynamicColumnValue))
                {
                    XElement _xDynamicSearchParameter = new("_xDynamicSearchParameter");
                    _xDynamicSearchParameter.Add(new XAttribute("DynamicColumnName", _dynamicSearchParameters.DynamicColumnName != null ? _dynamicSearchParameters.DynamicColumnName.ToString() : ""));
                    _xDynamicSearchParameter.Add(new XAttribute("DynamicColumnValue", _dynamicSearchParameters.DynamicColumnValue != null ? _dynamicSearchParameters.DynamicColumnValue.ToString() : ""));
                    _xDynamicSearchParameter.Add(new XAttribute("Operator", _dynamicSearchParameters.DynamicColumnOprator != null ? _dynamicSearchParameters.DynamicColumnOprator.ToString() : ""));
                    xDynamicSearchParameters.Add(_xDynamicSearchParameter);
                }
            }
            dynamicSearchParameters = xDynamicSearchParameters?.ToString() ?? string.Empty;
        }
    }
}
