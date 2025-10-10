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
        string code,
        int position,
        string? parentCode)
    {
        if (!RegexStatic.IsSnakeCaseRegex().IsMatch(code))
        {
            throw new DomainException(E.KeyFormatInvalid(code));
        }

        if (Options.Any(p => p.Code == code))
        {
            throw new DomainException(E.EnumOptAlreadyExists(code));
        }

        if (DependsOn is null &&
            parentCode is not null)
        {
            throw new DomainException(E.ScopeOptReqDep);
        }

        var opt = new EnumAttributeOption(this, code, position);

        if (DependsOn is EnumAttributeDefinition parentAttr)
        {
            if (parentCode is null)
            {
                throw new DomainException(E.DepReqScopeOtp);
            }

            if (parentAttr.Options.SingleOrDefault(p => p.Code == parentCode) is not { } parentOption)
            {
                throw new DomainException(E.EnumOptNotFound(parentCode));
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

        var optCodeSet = new HashSet<string>(StringComparer.Ordinal);
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
                    throw new DomainException(E.EnumOptNotScopedButDep(Key, DependsOn.Key, opt.Code));
                }
                if (opt.ParentOption.EnumAttributeDefinition != DependsOn)
                {
                    throw new DomainException(E.InvalidScopeParent(Key, DependsOn.Key, opt.Code, opt.ParentOption.EnumAttributeDefinition.Key));
                }
            }
            else
            {
                if (opt.ParentOption is not null)
                {
                    throw new DomainException(E.OptScopedButNoDep(Key, opt.Code));
                }
            }

            // All options must have unique codes
            if (!optCodeSet.Add(opt.Code))
            {
                throw new DomainException(E.DuplicateEnumOptCode(Key, opt.Code));
            }

            // All options must have unique positions
            if (!optPosSet.Add(opt.Position))
            {
                throw new DomainException(E.DuplicateEnumOptPosition(Key, opt.Code));
            }
        }
    }

    protected override string DebuggerDisplay
        => base.DebuggerDisplay;
}
