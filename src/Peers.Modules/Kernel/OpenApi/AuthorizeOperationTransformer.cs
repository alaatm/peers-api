using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Peers.Modules.Kernel.OpenApi;

/// <summary>
/// An OpenAPI operation transformer that adds bearer security requirements to operations with the "Protected" tag.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class AuthorizeOperationTransformer : IOpenApiOperationTransformer
{
    private const string ProtectedTag = "Protected";

    public Task TransformAsync(OpenApiOperation op, OpenApiOperationTransformerContext ctx, CancellationToken _)
    {
        var hasProtectedTag = ctx
            .Description
            .ActionDescriptor
            .EndpointMetadata
            .OfType<ITagsMetadata>()
            .SelectMany(m => m.Tags ?? [])
            .Any(t => string.Equals(t, ProtectedTag, StringComparison.OrdinalIgnoreCase));

        if (hasProtectedTag)
        {
            op.Security ??= [];
            op.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", ctx.Document)] = [],
            });
        }

        return Task.CompletedTask;
    }
}
