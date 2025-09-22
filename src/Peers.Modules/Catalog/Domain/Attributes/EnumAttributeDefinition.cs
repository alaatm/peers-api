using System.Diagnostics;
using Peers.Core.Domain.Errors;
using E = Peers.Modules.Catalog.CatalogErrors;

namespace Peers.Modules.Catalog.Domain.Attributes;

/// <summary>
/// Represents an attribute definition whose value is selected from a predefined set of options, typically used for
/// enumerated product attributes.
/// </summary>
/// <remarks>EnumAttributeDefinition is used to define attributes that have a fixed list of selectable options,
/// such as color or size. Each option is represented by an EnumAttributeOption. This class supports defining dependencies
/// on other enum attribute definitions, enabling hierarchical or scoped option selection.</remarks>
public sealed class EnumAttributeDefinition : DependentAttributeDefinition
{
    /// <summary>
    /// Indicates whether this attribute's value creates a unique, sellable variant of a listing.
    /// </summary>
    public bool IsVariant { get; private set; }

    /// <summary>
    /// The list of options associated with this attribute definition.
    /// </summary>
    public List<EnumAttributeOption> Options { get; private set; } = default!;

    private EnumAttributeDefinition() : base() { }

    internal EnumAttributeDefinition(
        ProductType owner,
        string key,
        bool isRequired,
        int position,
        bool isVariant) : base(owner, key, AttributeKind.Enum, isRequired, position)
    {
        IsVariant = isVariant;
        Options = [];
    }

    internal EnumAttributeOption AddOption(
        string key,
        int position,
        string? parentOptionKey)
    {
        if (!RegexStatic.IsSnakeCaseRegex().IsMatch(key))
        {
            throw new DomainException(E.KeyFormatInvalid(key));
        }

        if (Options.Any(o => o.Key == key))
        {
            throw new DomainException(E.OptAlreadyExists(key));
        }

        if (DependsOn is null &&
            parentOptionKey is not null)
        {
            throw new DomainException(E.ScopeOptReqDep);
        }

        var opt = new EnumAttributeOption(this, key, position);

        if (DependsOn is EnumAttributeDefinition dependsOnEnum)
        {
            if (parentOptionKey is null)
            {
                throw new DomainException(E.DepReqScopeOtp);
            }

            if (dependsOnEnum.Options.SingleOrDefault(o => o.Key == parentOptionKey) is not { } parentOption)
            {
                throw new DomainException(E.OptNotFound(parentOptionKey));
            }

            opt.ScopeTo(parentOption);
        }
        else
        {
            // Sanity assert. Either enum or null. Can't be lookup because of the allowed dependency matrix.
            Debug.Assert(DependsOn is null);
        }

        Options.Add(opt);
        return opt;
    }

    internal override void SetDependency(DependentAttributeDefinition parent)
    {
        Debug.Assert(Options.Count == 0);
        base.SetDependency(parent);
    }

    internal override void ClearDependency()
    {
        foreach (var opt in Options)
        {
            opt.ClearScope();
        }

        base.ClearDependency();
    }

    protected override string DebuggerDisplay
        => $"{base.DebuggerDisplay} | {(IsVariant ? "Variant" : "Non-variant")}";
}
