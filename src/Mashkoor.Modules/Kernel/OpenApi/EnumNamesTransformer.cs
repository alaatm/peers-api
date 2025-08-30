using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Mashkoor.Modules.Kernel.OpenApi;

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
            schema.Enum = [.. values.Select(v => (IOpenApiAny)new OpenApiInteger(v))];
        }
        schema.Type = "integer";
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
            return mi.GetCustomAttribute<DisplayAttribute>()?.Description
                ?? mi.GetCustomAttribute<DescriptionAttribute>()?.Description
                ?? string.Empty;
        }).ToArray();

        // Add vendor extensions many tools/UIs understand
        var enumNames = new OpenApiArray();
        enumNames.AddRange(names.Select(n => (IOpenApiAny)new OpenApiString(n)));
        schema.Extensions["x-enumNames"] = enumNames;

        var enumVarNames = new OpenApiArray();
        enumVarNames.AddRange(names.Select(n => (IOpenApiAny)new OpenApiString(n)));
        schema.Extensions["x-enum-varnames"] = enumVarNames;

        if (descs.Any(d => !string.IsNullOrEmpty(d)))
        {
            var xEnumDescs = new OpenApiArray();
            xEnumDescs.AddRange(descs.Select(d => (IOpenApiAny)new OpenApiString(d)));
            schema.Extensions["x-enum-descriptions"] = xEnumDescs;
        }

        return Task.CompletedTask;
    }
}
