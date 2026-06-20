using CoreAppwithSSO.ElectionTracker.Common;
using CoreAppwithSSO.ElectionTracker.Constants;
using CoreAppwithSSO.ElectionTracker.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using System.Runtime.Serialization;

namespace CoreAppwithSSO.ElectionTracker.Extension
{
    public static class CommonExtensions
    {
        #region Public Methods
        public static void SetAuditFields<T>(this T request, int id, int userId, string? userName = null) where T : IAuditAttributes
        {
            if (id == 0)
            {
                request.CreatedBy = userId;
            }
            else
            {
                request.LastUpdatedBy = userId;
            }

        }

        public static void AddCustomValidationResponse(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errorList = context.ModelState.ToErrorDescriptions();
                    var response = new ATMResponse<object>
                    {
                        IsError = true,
                        ResponseCode = Constant.VALIDATION_ERROR,
                        ResponseMessageType = "ValidationError",
                        Errors = errorList,
                        Data = [],
                        Tokens = [],
                        Exceptions = []
                    };
                    return new BadRequestObjectResult(response);
                };
            });
        }

        public static DataTable ToDataTable<T>(this List<T> items)
        {
            var dataTable = new DataTable(typeof(T).Name);
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                // Handle nullable types
                var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                dataTable.Columns.Add(prop.Name, propType);
            }

            foreach (var item in items)
            {
                var values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(item) ?? DBNull.Value;
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static DataTable ToDataTable<T>(this
    IEnumerable<T> data,
    List<string>? excludeProperties = null)
        {
            excludeProperties ??= new();

            var table = new DataTable(typeof(T).Name);
            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                    !excludeProperties.Contains(p.Name) &&
                    !ExcludeParameter(p))
                .ToArray();

            // Build schema
            foreach (var prop in properties)
            {
                //var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                //table.Columns.Add(prop.Name, type);
                table.Columns.Add(prop.Name, GetTvpColumnType(prop));
            }

            // Populate rows
            foreach (var item in data)
            {
                var row = table.NewRow();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item);
                    row[prop.Name] = value ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }

            return table;
        }

        private static bool ExcludeParameter(PropertyInfo property)
        {
            return Attribute.IsDefined(property, typeof(NotMappedAttribute))
                || Attribute.IsDefined(property, typeof(JsonIgnoreAttribute))
                || Attribute.IsDefined(property, typeof(IgnoreDataMemberAttribute));
        }

        private static Type GetTvpColumnType(PropertyInfo property)
        {
            var name = property.Name;

            // TVP date columns are NVARCHAR in SQL
            if (name is "EffectiveStartDate" or "EffectiveEndDate")
                return typeof(string);

            // Numeric
            if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                return typeof(int);

            if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
                return typeof(decimal);

            // DateTime (real datetime columns)
            if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                return typeof(DateTime);

            // Default to string
            return typeof(string);
        }

        #endregion

        #region Private Methods
        private static List<ErrorDescription> ToErrorDescriptions(this ModelStateDictionary modelState)
        {
            return [.. modelState
                .Where(ms => (ms.Value?.Errors?.Count ?? 0) > 0)
                .SelectMany(ms =>
                    ms.Value?.Errors?.Select(error =>
                        new ErrorDescription
                        {
                            ErrorCode = "ERR_VALIDATION_" + ms.Key.ToUpperInvariant(),
                            ErrorMessage = error.ErrorMessage
                        }
                    ) ?? []
                )];
        }
        #endregion
    }
}
