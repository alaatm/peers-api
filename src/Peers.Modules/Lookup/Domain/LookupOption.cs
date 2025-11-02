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

    private LookupOption()
    {
    }

    /// <summary>
    /// Initializes a new instance of the LookupOption class with the specified code and lookup type.
    /// </summary>
    /// <param name="code">The unique code that identifies the lookup option.</param>
    /// <param name="type">The type of the lookup option to associate with this instance.</param>
    internal LookupOption(string code, LookupType type)
    {
        Code = code;
        Type = type;
        Translations = [];
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string D => $"LO:{Id} - {Code} → {Type?.Key ?? TypeId.ToString(CultureInfo.InvariantCulture)}";
}
