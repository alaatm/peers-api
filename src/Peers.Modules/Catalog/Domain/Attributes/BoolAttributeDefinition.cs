namespace Peers.Modules.Catalog.Domain.Attributes;

/// <summary>
/// Represents a definition for a boolean attribute that can be associated with a product type.
/// </summary>
public sealed class BoolAttributeDefinition : AttributeDefinition
{
    private BoolAttributeDefinition() : base() { }

    internal BoolAttributeDefinition(
        ProductType owner,
        string key,
        bool isRequired,
        int position) : base(owner, key, AttributeKind.Bool, isRequired, false, position)
    {
    }
}
