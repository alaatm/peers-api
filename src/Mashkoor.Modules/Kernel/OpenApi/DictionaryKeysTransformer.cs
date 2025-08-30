using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Mashkoor.Modules.Kernel.OpenApi;

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
            var dictSchema = schema.Properties["data"].Properties["metadata"];
            dictSchema.AdditionalProperties.Extensions["x-additionalPropertiesName"] = new OpenApiString("<fileName>");
            dictSchema.Description = string.IsNullOrWhiteSpace(dictSchema.Description)
                ? "Keys are file names matching uploaded files; values are file metadata."
                : dictSchema.Description + "\n\nKeys are file names matching uploaded files; values are file metadata.";
        }
        else if (type == typeof(Media.Queries.GetStatus.Response))
        {
            var dictSchema = schema.Properties["status"];
            dictSchema.AdditionalProperties.Extensions["x-additionalPropertiesName"] = new OpenApiString("<mediaUrl>");
            dictSchema.Description = string.IsNullOrWhiteSpace(dictSchema.Description)
                ? "Keys are media URLs; values are upload statuses."
                : dictSchema.Description + "\n\nKeys are media URLs; values are upload statuses.";
        }
        else if (type == typeof(Users.Commands.DispatchMessage.Command))
        {
            var dictSchema = schema.Properties["title"];
            dictSchema.AdditionalProperties.Extensions["x-additionalPropertiesName"] = new OpenApiString("<lang>");
            dictSchema.Description = string.IsNullOrWhiteSpace(dictSchema.Description)
                ? "Keys are ISO lang code (en, ar, etc..); values are the localized value."
                : dictSchema.Description + "\n\nKeys are ISO lang code (en, ar, etc..); values are the localized value.";

            dictSchema = schema.Properties["body"];
            dictSchema.AdditionalProperties.Extensions["x-additionalPropertiesName"] = new OpenApiString("<lang>");
            dictSchema.Description = string.IsNullOrWhiteSpace(dictSchema.Description)
                ? "Keys are ISO lang code (en, ar, etc..); values are the localized value."
                : dictSchema.Description + "\n\nKeys are ISO lang code (en, ar, etc..); values are the localized value.";
        }

        return Task.CompletedTask;
    }
}
