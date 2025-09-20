using System.Diagnostics;
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
        int position) : base(owner, key, kind, isRequired, position)
    {
    }

    internal virtual void SetDependency(DependentAttributeDefinition parent)
    {
        Debug.Assert(ReferenceEquals(parent.ProductType, ProductType));
        Debug.Assert(parent != this);

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
            throw new DomainException(E.DependencyCombinationNotSupported(
                childKey: Key, childKind: Kind, parentKey: parent.Key, parentKind: parent.Kind));
        }

        // Prevent cycles
        for (var anc = parent; anc is not null; anc = anc.DependsOn)
        {
            if (anc == this)
            {
                throw new DomainException(E.CyclicDependency);
            }
        }

        DependsOn = parent;
    }

    internal virtual void ClearDependency()
    {
        DependsOn = null;
        DependsOnId = null;
    }

    protected override string DebuggerDisplay
        => $"{base.DebuggerDisplay} | {(DependsOn != null || DependsOnId != null ? $"Dependent ({DependsOn?.Key ?? DependsOnId!.Value.ToString(CultureInfo.InvariantCulture)})" : "Independent")}";
}
