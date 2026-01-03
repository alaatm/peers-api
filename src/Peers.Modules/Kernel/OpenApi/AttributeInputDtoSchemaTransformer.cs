using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Peers.Modules.Listings.Commands;

namespace Peers.Modules.Kernel.OpenApi;

[ExcludeFromCodeCoverage]
internal sealed class AttributeInputDtoSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken _)
    {
        if (context.JsonTypeInfo.Type != typeof(SetAttributes.Command))
        {
            return Task.CompletedTask;
        }

        schema.Type = JsonSchemaType.Object;
        schema.Format = null;

        schema.Title = "SetAttributes.Command";
        schema.Description =
            "Discriminated input for setting listing attributes and variant axes.\n\n" +
            "Each entry maps an attribute key (or a group key) to exactly one supported shape:\n" +
            "- Scalar shapes are for non-variant attributes.\n" +
            "- Axis shapes are for variant attributes (or groups).\n" +
            "- Mixing scalar and axis shapes for the same key is not allowed.";

        schema.Properties = new Dictionary<string, IOpenApiSchema>(StringComparer.OrdinalIgnoreCase)
        {
            ["snapshotId"] = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Description = "Snapshot identifier to apply the attribute changes against."
            },
            ["attributes"] = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Description =
                    "Map of attribute/group keys to a single discriminated value shape. " +
                    "Keys must match the product type definitions; values must be valid for the definition.",
                AdditionalPropertiesAllowed = true,
                AdditionalProperties = new OpenApiSchema
                {
                    OneOf =
                    [
                        // Scalar shapes (non-variant)
                        Numeric(),
                        Bool(),
                        Date(),
                        OptionCodeOrScalarString(),

                        // Axis shapes (variant)
                        NumericAxis(),
                        OptionCodeAxis(),
                        GroupAxis(),
                    ],
                },
                Example = JsonNode.Parse("""
                {
                  "condition": "new",
                  "color": ["black","white"],
                  "pack_size": [24,54,72],
                  "size": [[100,300],[150,400]]
                }
                """)
            }
        };

        schema.Required = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "snapshotId",
            "attributes"
        };

        // Ensure we don't leak old generated stuff
        schema.OneOf?.Clear();
        schema.AllOf?.Clear();
        schema.AnyOf?.Clear();
        schema.AdditionalPropertiesAllowed = false;

        return Task.CompletedTask;
    }

    // ---- Shape schemas ----
    // Adjust these to exactly match your JsonConverter’s supported shapes.

    private static OpenApiSchema Numeric() => new()
    {
        Type = JsonSchemaType.Number,
        Format = "double",
        Description = "Numeric scalar (non-variant attribute)."
    };

    private static OpenApiSchema Bool() => new()
    {
        Type = JsonSchemaType.Boolean,
        Description = "Boolean scalar (non-variant attribute)."
    };

    private static OpenApiSchema Date() => new()
    {
        Type = JsonSchemaType.String,
        Format = "date",
        Description = "Date scalar (non-variant attribute)."
    };

    private static OpenApiSchema OptionCodeOrScalarString() => new()
    {
        Type = JsonSchemaType.String,
        Description = "Option code or scalar string (non-variant attribute)."
    };

    private static OpenApiSchema NumericAxis() => new()
    {
        Type = JsonSchemaType.Array,
        Description = "Numeric axis (variant attribute). Items are either numeric values or numeric ranges.",
        Items = new OpenApiSchema
        {
            OneOf =
            [
                new OpenApiSchema { Type = JsonSchemaType.Number, Format = "double" }, // e.g. 24
                new OpenApiSchema                                      // e.g. [100,300]
                {
                    Type = JsonSchemaType.Array,
                    MinItems = 2,
                    MaxItems = 2,
                    Items = new OpenApiSchema { Type = JsonSchemaType.Number, Format = "double" }
                },
            ],
        }
    };

    private static OpenApiSchema OptionCodeAxis() => new()
    {
        Type = JsonSchemaType.Array,
        Description = "Option code axis (variant attribute).",
        Items = new OpenApiSchema { Type = JsonSchemaType.String }
    };

    private static OpenApiSchema GroupAxis() => new()
    {
        // This one depends on how your converter represents a "group axis".
        // If it’s also an array of option codes, keep it like OptionCodeAxis.
        // If it’s an object, model it as an object with additionalProperties, etc.
        Type = JsonSchemaType.Array,
        Description = "Group axis (variant group).",
        Items = new OpenApiSchema { Type = JsonSchemaType.String }
    };
}
