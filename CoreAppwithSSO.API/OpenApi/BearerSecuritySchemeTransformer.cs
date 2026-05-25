using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CoreAppwithSSO.API.OpenApi;

internal sealed class BearerSecuritySchemeTransformer : IDocumentFilter
{
    private const string BearerId = "Bearer";
    private const string ApiKeyId = "ApiKey";

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Components ??= new OpenApiComponents();
        swaggerDoc.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

        swaggerDoc.Components.SecuritySchemes[BearerId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token (without the 'Bearer ' prefix)."
        };

        swaggerDoc.Components.SecuritySchemes[ApiKeyId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            Name = "X-Api-Key",
            In = ParameterLocation.Header,
            Description = "Machine-to-machine key issued via /api/tenant/api-keys. Either Bearer or X-Api-Key is required."
        };

        swaggerDoc.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();
        swaggerDoc.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Id = BearerId, Type = ReferenceType.SecurityScheme }
            }] = Array.Empty<string>()
        });
        swaggerDoc.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Id = ApiKeyId, Type = ReferenceType.SecurityScheme }
            }] = Array.Empty<string>()
        });
    }
}
