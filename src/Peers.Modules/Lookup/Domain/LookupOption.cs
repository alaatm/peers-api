using System.Diagnostics;
using System.Globalization;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Lookup.Domain.Translations;

namespace Peers.Modules.Lookup.Domain;

/// <summary>
/// A concrete item within a <see cref="LookupType"/> (e.g., "samsung" under "brand" type).
/// </summary>
/// <remarks>
/// - Uniqueness: (<see cref="TypeId"/>, <see cref="Code"/>) must be unique and stable.
/// - Used by listings and product type allow-lists
/// - If you support aliases/translations, they hang off this value.
/// </remarks>
[DebuggerDisplay("{D,nq}")]
public sealed class LookupOption : Entity, ILocalizable<LookupOption, LookupOptionTr>
{
    /// <summary>
    /// Stable ASCII code for this value (e.g., "samsung", "galaxy_a5").
    /// </summary>
    public string Code { get; set; } = default!;
    /// <summary>
    /// The unique identifier for the type.
    /// </summary>
    public int TypeId { get; set; }
    /// <summary>
    /// The associated lookup type.
    /// </summary>
    public LookupType Type { get; set; } = default!;
    /// <summary>
    /// The list of translations associated with this lookup option.
    /// </summary>
    public List<LookupOptionTr> Translations { get; private set; } = default!;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string D => $"LO:{Id} - {Code} → {Type?.Key ?? TypeId.ToString(CultureInfo.InvariantCulture)}";
}
