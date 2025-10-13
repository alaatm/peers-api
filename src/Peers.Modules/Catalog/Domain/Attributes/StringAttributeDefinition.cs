using System.Text.RegularExpressions;
using Peers.Core.Domain.Errors;
using E = Peers.Modules.Catalog.CatalogErrors;

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
        string? regex) : base(owner, key, AttributeKind.String, isRequired, false, position)
        => Config = new(regex);

    public void ValidateValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            (Config.Regex is string r && !Regex.IsMatch(value, r)))
        {
            throw new DomainException(E.AttrValueMustBeValidString(Key, value ?? "", Config.Regex));
        }
    }
}
