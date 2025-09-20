namespace Peers.Modules.Catalog.Domain.Attributes;

/// <summary>
/// Represents a definition for a date attribute that can be associated with a product type.
/// </summary>
public sealed class DateAttributeDefinition : AttributeDefinition
{
    private DateAttributeDefinition() : base() { }

    internal DateAttributeDefinition(
        ProductType owner,
        string key,
        bool isRequired,
        int position) : base(owner, key, AttributeKind.Date, isRequired, position)
    {
    }
}
