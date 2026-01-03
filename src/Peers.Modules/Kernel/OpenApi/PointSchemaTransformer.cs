using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using NetTopologySuite.Geometries;

namespace Peers.Modules.Kernel.OpenApi;

[ExcludeFromCodeCoverage]
internal sealed class PointSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken _)
    {
        if (context.JsonTypeInfo.Type != typeof(Point))
        {
            return Task.CompletedTask;
        }

        schema.Type = JsonSchemaType.Object;
        schema.Format = null;
        schema.Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["lat"] = new OpenApiSchema { Type = JsonSchemaType.Number, Format = "double", Description = "The latitude" },
            ["lon"] = new OpenApiSchema { Type = JsonSchemaType.Number, Format = "double", Description = "The longitude" }
        };
        schema.Required = new HashSet<string> { "lat", "lon" };

        return Task.CompletedTask;
    }
}
