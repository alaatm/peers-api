using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Listings.Domain.Validation;

namespace Peers.Modules.Listings.Domain.Snapshots;

public partial record ListingSnapshot
{
    internal void Validate(ValidationContext ctx)
    {
        // Attributes validation
        //

        // Axes validation
        var schemaAxesDefSet = new HashSet<AttributeDefinition>();

        foreach (var axis in Axes)
        {
            axis.ValidateSchema(ctx, schemaAxesDefSet);
        }

        foreach (var axis in Axes)
        {
            foreach (var variant in ctx.Variants)
            {
                var variantAttrsByDef = variant.Attributes.ToDictionary(p => p.AttributeDefinition);
                axis.ValidateVariantCoverage(ctx, variant, variantAttrsByDef, schemaAxesDefSet);
            }
        }
        //
    }
}
