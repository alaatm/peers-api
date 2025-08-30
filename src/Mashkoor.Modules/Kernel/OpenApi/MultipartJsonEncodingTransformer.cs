using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Mashkoor.Modules.Kernel.OpenApi;

/// <summary>
/// An OpenAPI document transformer that sets encoding for multipart/form-data command props.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MultipartJsonEncodingTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation op, OpenApiOperationTransformerContext ctx, CancellationToken _)
    {
        if (op.RequestBody?.Content.TryGetValue("multipart/form-data", out var fd) ?? false)
        {
            fd.Encoding["data"] = new OpenApiEncoding { ContentType = "application/json" };
            fd.Encoding["files"] = new OpenApiEncoding { Style = ParameterStyle.Form, Explode = true };
        }

        return Task.CompletedTask;
    }
}
