using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Peers.Modules.Kernel.OpenApi;

/// <summary>
/// An OpenAPI document transformer that adds a additional prop extensions to dictionaries in endpoints commands.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class DictionaryKeysTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext ctx, CancellationToken _)
    {
        var type = ctx.JsonTypeInfo.Type;

        if (type == typeof(Media.Commands.Upload.CommandDoc))
        {
            PutKeyName(schema.Properties?["data"]?.Properties?["metadata"] as OpenApiSchema, "<fileName>");
        }
        else if (type == typeof(Media.Queries.GetStatus.Response))
        {
            PutKeyName(schema.Properties?["status"] as OpenApiSchema, "<mediaUrl>");
        }
        else if (type == typeof(Users.Commands.DispatchMessage.Command))
        {
            PutKeyName(schema.Properties?["title"] as OpenApiSchema, "<lang>");
            PutKeyName(schema.Properties?["body"] as OpenApiSchema, "<lang>");
        }

        return Task.CompletedTask;

        static void PutKeyName(OpenApiSchema? schema, string keyName)
        {
            if (schema?.AdditionalProperties is OpenApiSchema props)
            {
                props.Extensions ??= new Dictionary<string, IOpenApiExtension>();
                props.Extensions.Add("x-additionalPropertiesName", new JsonNodeExtension(keyName));
            }
        }
    }
}
