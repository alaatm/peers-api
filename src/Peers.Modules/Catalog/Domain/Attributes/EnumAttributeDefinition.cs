using System.Diagnostics;
using System.Globalization;
using Peers.Core.Domain.Errors;
using E = Peers.Modules.Catalog.CatalogErrors;

namespace Peers.Modules.Catalog.Domain.Attributes;

/// <summary>
/// Represents an attribute definition whose value is selected from a predefined set of options, typically used for
/// enumerated product attributes.
/// </summary>
/// <remarks>
/// EnumAttributeDefinition is used to define attributes that have a fixed list of selectable options,
/// such as color or size. Each option is represented by an EnumAttributeOption. This class supports defining dependencies
/// on other enum attribute definitions, enabling hierarchical or scoped option selection.
/// </remarks>
public sealed class EnumAttributeDefinition : AttributeDefinition
{
    /// <summary>
    /// The identifier of another attribute definition that this attribute depends on, if any.
    /// </summary>
    public int? DependsOnId { get; private set; }
    /// <summary>
    /// The other attribute definition that this attribute depends on, if any.
    /// </summary>
    public EnumAttributeDefinition? DependsOn { get; private set; }
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

        if (DependsOn is { } parentAttr)
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

    internal void SetDependency(EnumAttributeDefinition parent)
    {
        if (Options.Count != 0)
        {
            throw new DomainException(E.EnumAttrDepSetOnlyIfNoOpts(Key));
        }

        ValidateDependency(parent);
        DependsOn = parent;
    }

    internal void ClearDependency()
    {
        foreach (var opt in Options)
        {
            opt.ClearScope();
        }

        DependsOn = null;
        DependsOnId = null;
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

        if (DependsOn is not null)
        {
            ValidateDependency(DependsOn);
        }
    }

    private void ValidateDependency(EnumAttributeDefinition parent)
    {
        if (parent.ProductType != ProductType)
        {
            throw new DomainException(E.DepDiffProductType(childKey: Key, parentKey: parent.Key));
        }
        if (parent == this)
        {
            throw new DomainException(E.SelfDep(Key));
        }

        // Ensure the following:
        // 1. If parent is variant, child must be variant too.
        // 2. if parent is not variant, child can be either.
        // 3. If child is required, parent must be required too.
        // 4. If child is not required, parent can be either.
        if (parent.IsVariant && !IsVariant)
        {
            throw new DomainException(E.VariantDependencyViolation(childKey: Key, parentKey: parent.Key));
        }
        if (IsRequired && !parent.IsRequired)
        {
            throw new DomainException(E.RequiredDependencyViolation(childKey: Key, parentKey: parent.Key));
        }

        // Prevent cycles
        for (var anc = parent; anc is not null; anc = anc.DependsOn)
        {
            if (anc == this)
            {
                throw new DomainException(E.CyclicDependency);
            }
        }
    }

    public override string D
        => $"{base.D} | {(DependsOn != null || DependsOnId != null ? $"Dependent ({DependsOn?.Key ?? DependsOnId!.Value.ToString(CultureInfo.InvariantCulture)})" : "Independent")} | {Options.Count} options";
}
