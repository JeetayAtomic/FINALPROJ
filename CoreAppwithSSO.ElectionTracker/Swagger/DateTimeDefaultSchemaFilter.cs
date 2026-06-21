using CoreAppwithSSO.ElectionTracker.Common;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace CoreAppwithSSO.ElectionTracker.Swagger
{
    public class DateTimeDefaultSchemaFilter : ISchemaFilter
    {
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties == null || context.Type == null)
            {
                return;
            }

            var now = DateTime.Now.ToString(DateFormat);

            foreach (var property in context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.GetCustomAttribute<CommonUtl.DateTimeParametersAttribute>() == null)
                {
                    continue;
                }

                // Swashbuckle camel-cases property names by default, so match case-insensitively.
                var key = schema.Properties.Keys.FirstOrDefault(
                    k => string.Equals(k, property.Name, StringComparison.OrdinalIgnoreCase));

                if (key != null)
                {
                    schema.Properties[key].Example = new OpenApiString(now);
                    schema.Properties[key].Default = new OpenApiString(now);
                }
            }
        }
    }
}
