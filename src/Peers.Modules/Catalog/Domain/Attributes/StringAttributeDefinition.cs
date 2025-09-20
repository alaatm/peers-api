namespace Peers.Modules.Catalog.Domain.Attributes;

/// <summary>
/// Represents the definition of a string attribute for a product type.
/// </summary>
public sealed class StringAttributeDefinition : AttributeDefinition
{
    public StringAttrConfig Config { get; set; }

    private StringAttributeDefinition() : base() { }

    internal StringAttributeDefinition(
        ProductType owner,
        string key,
        bool isRequired,
        int position,
        string? regex) : base(owner, key, AttributeKind.String, isRequired, position)
        => Config = new(regex);
}
