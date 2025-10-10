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
    /// The list of options associated with this attribute definition.
    /// </summary>
    public List<EnumAttributeOption> Options { get; private set; } = default!;

    private EnumAttributeDefinition() : base() { }

    internal EnumAttributeDefinition(
        ProductType owner,
        string key,
        bool isRequired,
        bool isVariant,
        int position) : base(owner, key, AttributeKind.Enum, isRequired, isVariant, position)
        => Options = [];

    internal EnumAttributeOption AddOption(
        string key,
        int position,
        string? parentOptKey)
    {
        if (!RegexStatic.IsSnakeCaseRegex().IsMatch(key))
        {
            throw new DomainException(E.KeyFormatInvalid(key));
        }

        if (Options.Any(p => p.Key == key))
        {
            throw new DomainException(E.EnumOptAlreadyExists(key));
        }

        if (DependsOn is null &&
            parentOptKey is not null)
        {
            throw new DomainException(E.ScopeOptReqDep);
        }

        var opt = new EnumAttributeOption(this, key, position);

        if (DependsOn is EnumAttributeDefinition parentAttr)
        {
            if (parentOptKey is null)
            {
                throw new DomainException(E.DepReqScopeOtp);
            }

            if (parentAttr.Options.SingleOrDefault(p => p.Key == parentOptKey) is not { } parentOption)
            {
                throw new DomainException(E.EnumOptNotFound(parentOptKey));
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
        if (Options.Count != 0)
        {
            throw new DomainException(E.EnumAttrDepSetOnlyIfNoOpts(Key));
        }

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

    internal override void Validate()
    {
        base.Validate();

        // Must have at least one option
        if (Options.Count == 0)
        {
            throw new DomainException(E.EnumAttrNoOptions(Key));
        }

        var optKeySet = new HashSet<string>(StringComparer.Ordinal);
        var optPosSet = new HashSet<int>();
        var hasDependency = DependsOn is not null;

        foreach (var opt in Options)
        {
            // Consistency: if DependsOn set => all options must be scoped; if null => none may be scoped
            if (hasDependency)
            {
                Debug.Assert(DependsOn is not null);

                if (opt.ParentOption is null)
                {
                    throw new DomainException(E.EnumOptNotScopedButDep(Key, DependsOn.Key, opt.Key));
                }
                if (opt.ParentOption.EnumAttributeDefinition != DependsOn)
                {
                    throw new DomainException(E.InvalidScopeParent(Key, DependsOn.Key, opt.Key, opt.ParentOption.EnumAttributeDefinition.Key));
                }
            }
            else
            {
                if (opt.ParentOption is not null)
                {
                    throw new DomainException(E.OptScopedButNoDep(Key, opt.Key));
                }
            }

            // All options must have unique keys
            if (!optKeySet.Add(opt.Key))
            {
                throw new DomainException(E.DuplicateEnumOptionKey(Key, opt.Key));
            }

            // All options must have unique positions
            if (!optPosSet.Add(opt.Position))
            {
                throw new DomainException(E.DuplicateEnumOptionPosition(Key, opt.Key));
            }
        }
    }

    protected override string DebuggerDisplay
        => base.DebuggerDisplay;
}
