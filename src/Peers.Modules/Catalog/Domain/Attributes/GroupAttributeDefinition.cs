using Peers.Core.Domain.Errors;
using E = Peers.Modules.Catalog.CatalogErrors;

namespace Peers.Modules.Catalog.Domain.Attributes;

/// <summary>
/// Composite (virtual) variant axis that groups multiple numeric attributes into a single selectable "size"-like option.
/// </summary>
/// <remarks>
/// <para>
/// A <c>GroupAttributeDefinition</c> declares an ordered list of member <see cref="NumericAttributeDefinition"/>
/// (e.g., <c>width</c>, <c>length</c>) and is used only to author and present tuples such as "100×300".
/// It never holds a scalar value itself and must never appear in <c>ListingAttribute</c> or <c>ListingVariantAttribute</c>.
/// Per-SKU storage uses the group’s <em>member</em> attributes (e.g., store <c>width=100</c> and <c>length=300</c>).
/// </para>
///
/// <para><strong>Invariants / constraints</strong></para>
/// <list type="bullet">
///   <item><description><strong>Kind:</strong> <see cref="AttributeKind.Group"/>; <c>IsVariant = true</c>; <c>IsRequired = false</c>.</description></item>
///   <item><description><strong>Numeric-only:</strong> all members are numeric and share the same <c>NumericKind</c> (all <c>Int</c> or all <c>Decimal</c>); mixing is not allowed.</description></item>
///   <item><description><strong>Same product type:</strong> every member belongs to the same <see cref="ProductType"/> as the group.</description></item>
///   <item><description><strong>Disjointness:</strong> a numeric attribute may belong to at most one group, and if a member is in a group it cannot also be an independent variant axis.</description></item>
///   <item><description><strong>No dependencies (v1):</strong> neither the group nor its members may use <c>DependsOn</c>.</description></item>
///   <item><description><strong>Units:</strong> members should share the same unit (normalized); validation rejects mismatches.</description></item>
///   <item><description><strong>Ordering:</strong> the group’s member order is defined by members’ <c>Position</c> (or a group-specific order if modeled);
///     this order is used for display and for building stable <c>VariantKey</c> segments.</description></item>
///   <item><description><strong>Publish rules:</strong> at least two distinct members; member set unique; all invariants re-validated on publish.</description></item>
/// </list>
///
/// <para><strong>UI and keys</strong></para>
/// <para>
/// The PDP renders one selector (e.g., "Size") using the group’s tuples. <c>VariantKey</c> segments are built from
/// the member attributes (e.g., <c>width=100|length=300</c>), not from the group itself, to keep search/sort trivial.
/// </para>
///
/// <example>
/// <code>
/// // PT: width (int, cm), length (int, cm), size group = [width, length]
/// // Listing tuples: (100,300), (150,400)
/// // SKUs store:
/// //   SKU1: width=100, length=300
/// //   SKU2: width=150, length=400
/// // UI shows: Size: 100×300 cm, 150×400 cm
/// </code>
/// </example>
/// </remarks>
public class GroupAttributeDefinition : AttributeDefinition
{
    public List<NumericAttributeDefinition> Members { get; set; } = default!;

    private GroupAttributeDefinition() : base() { }

    internal GroupAttributeDefinition(
        ProductType owner,
        string key,
        int position) : base(owner, key, AttributeKind.Group, false, true, position)
        => Members = [];

    internal void AddMember(NumericAttributeDefinition member)
    {
        if (member.ProductType != ProductType)
        {
            throw new DomainException(E.GroupAttrMemberProductTypeMismatch(Key, member.Key));
        }
        if (member.GroupDefinition is not null)
        {
            throw new DomainException(E.GroupAttrMemberAlreadyInGroup(Key, member.Key, member.GroupDefinition.Key));
        }
        if (member.IsVariant)
        {
            throw new DomainException(E.GroupAttrMemberMustNotBeVariant(Key, member.Key));
        }
        if (Members.Any(p => p == member))
        {
            throw new DomainException(E.GroupAttrMemberAlreadyExists(Key, member.Key));
        }
        if (Members.Count > 0)
        {
            if (member.NumericKind != Members[0].NumericKind)
            {
                throw new DomainException(E.GroupAttrMembersMixedNumericKind(Key));
            }
            if (member.Unit != Members[0].Unit)
            {
                throw new DomainException(E.GroupAttrMembersMixedUnits(Key));
            }
        }

        member.GroupDefinition = this;
        Members.Add(member);
    }

    internal override void Validate()
    {
        base.Validate();

        // Group must be variant and not required
        if (!IsVariant || IsRequired)
        {
            throw new DomainException(E.GroupAttrMustBeVariantNotRequired(Key));
        }

        // Have at least two members
        if (Members.Count < 2)
        {
            throw new DomainException(E.GroupAttrMinMembers(Key, 2));
        }

        var memberKeySet = new HashSet<string>(StringComparer.Ordinal);

        foreach (var member in Members)
        {
            // All members belong to this product type
            if (member.ProductType != ProductType)
            {
                throw new DomainException(E.GroupAttrMemberProductTypeMismatch(Key, member.Key));
            }

            // All members are numeric
            if (member.Kind is not (AttributeKind.Int or AttributeKind.Decimal))
            {
                throw new DomainException(E.GroupAttrMemberMustBeNumeric(Key, member.Key));
            }

            // All members share the same NumericKind
            if (member.NumericKind != Members[0].NumericKind)
            {
                throw new DomainException(E.GroupAttrMembersMixedNumericKind(Key));
            }

            // All members share the same unit (including null)
            if (member.Unit?.Trim().ToLowerInvariant() != Members[0].Unit?.Trim().ToLowerInvariant())
            {
                throw new DomainException(E.GroupAttrMembersMixedUnits(Key));
            }

            // All members must not be required - this affects listing header attribute validation
            // The member is always required if the group is used in a listing
            if (member.IsRequired)
            {
                throw new DomainException(E.GroupAttrMemberMustNotBeRequired(Key, member.Key));
            }

            // No member is also a variant axis
            if (member.IsVariant)
            {
                throw new DomainException(E.GroupAttrMemberMustNotBeVariant(Key, member.Key));
            }

            // No duplicate members
            if (!memberKeySet.Add(member.Key))
            {
                throw new DomainException(E.GroupAttrDuplicateMember(Key, member.Key));
            }
        }
    }
}
