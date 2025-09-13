using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Peers.Modules.Kernel.OpenApi;

/// <summary>
/// An OpenAPI document transformer that adds a Bearer security scheme to the OpenAPI document.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var requirements = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer", // "bearer" refers to the header name here
                In = ParameterLocation.Header,
                BearerFormat = "Json Web Token"
            }
        };
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = requirements;

        return Task.CompletedTask;
    }
}
