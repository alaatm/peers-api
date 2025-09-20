using System.Diagnostics;
using System.Globalization;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Catalog.Domain.Translations;

namespace Peers.Modules.Catalog.Domain.Attributes;

/// <summary>
/// Represents an option within an enum attribute definition, including its unique key, and hierarchical
/// relationships.
/// </summary>
/// <remarks>
/// This class is used to define selectable options for an attribute, such as color options ("Red",
/// "Green") or other enumerated values. Each option is uniquely identified by its <see cref="Key"/> and can be ordered
/// using the <see cref="Position"/> property. Options may also have a parent-child relationship, defined by <see
/// cref="ParentOption"/>.
/// </remarks>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class AttributeOption : Entity, ILocalizable<AttributeOption, AttributeOptionTr>
{
    /// <summary>
    /// The unique, non-localizable identifier (e.g., "red", "green")
    /// </summary>
    public string Key { get; private set; } = default!;
    /// <summary>
    /// The position of the option; used for stable ordering.
    /// </summary>
    public int Position { get; private set; }

    /// <summary>
    /// The identifier of the owning attribute definition.
    /// </summary>
    public int AttributeDefinitionId { get; private set; }
    /// <summary>
    /// The owning attribute definition.
    /// </summary>
    public EnumAttributeDefinition AttributeDefinition { get; private set; } = default!;

    /// <summary>
    /// The identifier of the parent option, if any.
    /// </summary>
    public int? ParentOptionId { get; private set; }
    /// <summary>
    /// The parent option, if any.
    /// </summary>
    public AttributeOption? ParentOption { get; private set; }
    /// <summary>
    /// The collection of translations associated with this attribute option.
    /// </summary>
    public List<AttributeOptionTr> Translations { get; private set; } = default!;

    private AttributeOption() { }

    internal AttributeOption(
        EnumAttributeDefinition owner,
        string key,
        int position)
    {
        AttributeDefinition = owner;
        Key = key;
        Position = position;
        Translations = [];
    }

    internal void ScopeTo(AttributeOption parentOption)
        => ParentOption = parentOption;

    internal void ClearScope()
    {
        ParentOption = null;
        ParentOptionId = null;
    }

    private string DebuggerDisplay => $"{Key} → {AttributeDefinition?.Key ?? AttributeDefinitionId.ToString(CultureInfo.InvariantCulture)} | {(ParentOption != null || ParentOptionId != null ? $"Scoped ({ParentOption?.Key ?? ParentOptionId!.Value.ToString(CultureInfo.InvariantCulture)} → {ParentOption?.AttributeDefinition?.Key ?? ParentOption?.AttributeDefinitionId.ToString(CultureInfo.InvariantCulture) ?? "<unloaded>"})" : "Unscoped")}";
}
