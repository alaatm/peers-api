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
public sealed class EnumAttributeOption : Entity, ILocalizable<EnumAttributeOption, EnumAttributeOptionTr>
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
    public int EnumAttributeDefinitionId { get; private set; }
    /// <summary>
    /// The owning attribute definition.
    /// </summary>
    public EnumAttributeDefinition EnumAttributeDefinition { get; private set; } = default!;

    /// <summary>
    /// The identifier of the parent option, if any.
    /// </summary>
    public int? ParentOptionId { get; private set; }
    /// <summary>
    /// The parent option, if any.
    /// </summary>
    public EnumAttributeOption? ParentOption { get; private set; }
    /// <summary>
    /// The collection of translations associated with this attribute option.
    /// </summary>
    public List<EnumAttributeOptionTr> Translations { get; private set; } = default!;

    private EnumAttributeOption() { }

    internal EnumAttributeOption(
        EnumAttributeDefinition owner,
        string key,
        int position)
    {
        EnumAttributeDefinition = owner;
        Key = key;
        Position = position;
        Translations = [];
    }

    internal void ScopeTo(EnumAttributeOption parentOption)
        => ParentOption = parentOption;

    internal void ClearScope()
    {
        ParentOption = null;
        ParentOptionId = null;
    }

    private string DebuggerDisplay => $"{Key} → {EnumAttributeDefinition?.Key ?? EnumAttributeDefinitionId.ToString(CultureInfo.InvariantCulture)} | {(ParentOption != null || ParentOptionId != null ? $"Scoped ({ParentOption?.Key ?? ParentOptionId!.Value.ToString(CultureInfo.InvariantCulture)} → {ParentOption?.EnumAttributeDefinition?.Key ?? ParentOption?.EnumAttributeDefinitionId.ToString(CultureInfo.InvariantCulture) ?? "<unloaded>"})" : "Unscoped")}";
}
