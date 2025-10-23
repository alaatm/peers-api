using System.Globalization;
using Peers.Core.Domain.Errors;
using E = Peers.Modules.Catalog.CatalogErrors;

namespace Peers.Modules.Catalog.Domain.Attributes;

/// <summary>
/// Represents an attribute definition that has a dependency on another attribute definition within the same product
/// type.
/// </summary>
/// <remarks>This is used to define attributes whose values or applicability depend on the value of another
/// attribute. Dependencies are restricted to compatible attribute kinds and must not form cycles.</remarks>
public abstract class DependentAttributeDefinition : AttributeDefinition
{
    /// <summary>
    /// The identifier of another attribute definition that this attribute depends on, if any.
    /// </summary>
    public int? DependsOnId { get; private set; }
    /// <summary>
    /// The other attribute definition that this attribute depends on, if any.
    /// </summary>
    public DependentAttributeDefinition? DependsOn { get; private set; }

    protected DependentAttributeDefinition() : base() { }

    protected DependentAttributeDefinition(
        ProductType owner,
        string key,
        AttributeKind kind,
        bool isRequired,
        bool isVariant,
        int position) : base(owner, key, kind, isRequired, isVariant, position)
    {
    }

    internal virtual void SetDependency(DependentAttributeDefinition parent)
    {
        ValidateDependency(parent);
        DependsOn = parent;
    }

    internal virtual void ClearDependency()
    {
        DependsOn = null;
        DependsOnId = null;
    }

    internal override void Validate()
    {
        base.Validate();
        if (DependsOn is not null)
        {
            ValidateDependency(DependsOn);
        }
    }

    private void ValidateDependency(DependentAttributeDefinition parent)
    {
        if (parent.ProductType != ProductType)
        {
            throw new DomainException(E.DepDiffProductType(childKey: Key, parentKey: parent.Key));
        }
        if (parent == this)
        {
            throw new DomainException(E.SelfDep(Key));
        }

        // Allowed matrix:
        // Enum(child)   -> Enum(parent)     OK
        // Lookup(child) -> Lookup(parent)   OK
        // Lookup(child) -> Enum(parent)     ERROR
        // Enum(child)   -> Lookup(parent)   ERROR
        var allowed =
            (Kind is AttributeKind.Enum && parent.Kind is AttributeKind.Enum) ||
            (Kind is AttributeKind.Lookup && parent.Kind is AttributeKind.Lookup);

        if (!allowed)
        {
            throw new DomainException(E.DepComboNotSupported(
                childKey: Key, childKind: Kind, parentKey: parent.Key, parentKind: parent.Kind));
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
        => $"{base.D} | {(DependsOn != null || DependsOnId != null ? $"Dependent ({DependsOn?.Key ?? DependsOnId!.Value.ToString(CultureInfo.InvariantCulture)})" : "Independent")}";
}
