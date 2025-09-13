using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using DA = System.ComponentModel.DataAnnotations;

namespace Peers.Modules.Kernel.OpenApi;

/// <summary>
/// An OpenAPI document transformer that adds enum names to commands with enums.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class EnumNamesTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext ctx, CancellationToken _)
    {
        var t = ctx.JsonTypeInfo?.Type;
        if (t is null)
        {
            return Task.CompletedTask;
        }

        t = Nullable.GetUnderlyingType(t) ?? t;
        if (!t.IsEnum)
        {
            return Task.CompletedTask;
        }

        // Ensure numeric enum values are present
        var values = Enum.GetValues(t).Cast<object>().Select(Convert.ToInt32).ToArray();
        if (schema.Enum is null || schema.Enum.Count == 0)
        {
            schema.Enum = [.. values.Select(v => JsonValue.Create(v))];
        }
        schema.Type = JsonSchemaType.Integer;
        schema.Format = "int32";

        // Build names (prefer customized names when available)
        var names = Enum.GetNames(t).Select(n =>
        {
            var mi = t.GetMember(n)[0];
            return mi.GetCustomAttribute<JsonStringEnumMemberNameAttribute>()?.Name
                ?? mi.GetCustomAttribute<EnumMemberAttribute>()?.Value
                ?? n;
        }).ToArray();

        // Optional: descriptions from [Display(Description=...)] or [Description]
        var descs = Enum.GetNames(t).Select(n =>
        {
            var mi = t.GetMember(n)[0];
            return mi.GetCustomAttribute<DA.DisplayAttribute>()?.Description
                ?? mi.GetCustomAttribute<DescriptionAttribute>()?.Description
                ?? string.Empty;
        }).ToArray();

        schema.Extensions ??= new Dictionary<string, IOpenApiExtension>();

        // Add vendor extensions many tools/UIs understand
        var enumNames = new JsonArray([.. names.Select(n => JsonValue.Create(n))]);
        schema.Extensions["x-enumNames"] = new JsonNodeExtension(enumNames);

        var enumVarNames = new JsonArray([.. names.Select(n => JsonValue.Create(n))]);
        schema.Extensions["x-enum-varnames"] = new JsonNodeExtension(enumVarNames);

        if (descs.Any(d => !string.IsNullOrEmpty(d)))
        {
            var xEnumDescs = new JsonArray([.. descs.Select(d => JsonValue.Create(d))]);
            schema.Extensions["x-enum-descriptions"] = new JsonNodeExtension(xEnumDescs);
        }

        return Task.CompletedTask;
    }
}
