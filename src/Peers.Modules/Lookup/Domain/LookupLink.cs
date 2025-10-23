using System.Diagnostics;
using System.Globalization;

namespace Peers.Modules.Lookup.Domain;

/// <summary>
/// Declares an allowed relationship between two lookup options, used to scope a child lookup
/// by a selected parent lookup (e.g., Brand → DeviceModel).
/// </summary>
/// <remarks>
/// - Acts like "scoped options" for lookups: a child value is valid only if a link exists
///   from the chosen parent value to that child value.
/// - DB enforces consistency via composite FKs to <c>LookupOption</c>:
///   (ParentTypeId, ParentOptionId) → (LookupOption.TypeId, LookupOption.Id) and
///   (ChildTypeId,  ChildOptionId)  → (LookupOption.TypeId, LookupOption.Id).
/// </remarks>
[DebuggerDisplay("{D,nq}")]
public sealed class LookupLink : IAggregateRoot
{
    /// <summary>
    /// The identifier of the parent lookup type (e.g., brand).
    /// </summary>
    public int ParentTypeId { get; private set; }
    /// <summary>
    /// The identifier of the parent lookup option (e.g., apple).
    /// </summary>
    public int ParentOptionId { get; private set; }
    /// <summary>
    /// The identifier of the child lookup type (e.g., device_model).
    /// </summary>
    public int ChildTypeId { get; private set; }
    /// <summary>
    /// The identifier of the child lookup option (e.g., iPhone 13).
    /// </summary>
    public int ChildOptionId { get; private set; }

    /// <summary>
    /// The parent lookup type (e.g., brand).
    /// </summary>
    public LookupType ParentType { get; set; } = default!;
    /// <summary>
    /// The child lookup type (e.g., device_model).
    /// </summary>
    public LookupType ChildType { get; set; } = default!;
    /// <summary>
    /// The parent lookup option (e.g., apple).
    /// </summary>
    public LookupOption ParentOption { get; set; } = default!;
    /// <summary>
    /// The child lookup option (e.g., iPhone 13).
    /// </summary>
    public LookupOption ChildOption { get; set; } = default!;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string D => $"{ChildOption?.Type?.Key ?? ChildTypeId.ToString(CultureInfo.InvariantCulture)} ({ChildOption?.Code ?? ChildOptionId.ToString(CultureInfo.InvariantCulture)}) → {ParentOption?.Type?.Key ?? ParentTypeId.ToString(CultureInfo.InvariantCulture)} ({ParentOption?.Code ?? ParentOptionId.ToString(CultureInfo.InvariantCulture)})";
}
