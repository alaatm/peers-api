using System.Diagnostics;
using System.Globalization;

namespace Peers.Modules.Lookup.Domain;

/// <summary>
/// Declares an allowed relationship between two lookup values, used to scope a child lookup
/// by a selected parent lookup (e.g., Brand → DeviceModel).
/// </summary>
/// <remarks>
/// - Acts like "scoped options" for lookups: a child value is valid only if a link exists
///   from the chosen parent value to that child value.
/// - DB enforces consistency via composite FKs to <c>LookupValue</c>:
///   (ParentTypeId, ParentValueId) → (LookupValue.TypeId, LookupValue.Id) and
///   (ChildTypeId,  ChildValueId)  → (LookupValue.TypeId, LookupValue.Id).
/// </remarks>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class LookupLink : IAggregateRoot
{
    /// <summary>
    /// The identifier of the parent lookup type (e.g., brand).
    /// </summary>
    public int ParentTypeId { get; private set; }
    /// <summary>
    /// The identifier of the parent lookup value (e.g., apple).
    /// </summary>
    public int ParentValueId { get; private set; }
    /// <summary>
    /// The identifier of the child lookup type (e.g., device_model).
    /// </summary>
    public int ChildTypeId { get; private set; }
    /// <summary>
    /// The identifier of the child lookup value (e.g., iPhone 13).
    /// </summary>
    public int ChildValueId { get; private set; }

    /// <summary>
    /// The parent lookup value (e.g., apple).
    /// </summary>
    public LookupValue ParentValue { get; set; } = default!;
    /// <summary>
    /// The child lookup value (e.g., iPhone 13).
    /// </summary>
    public LookupValue ChildValue { get; set; } = default!;

    private string DebuggerDisplay
        => $"{ChildValue?.Type?.Key ?? ChildTypeId.ToString(CultureInfo.InvariantCulture)} ({ChildValue?.Key ?? ChildValueId.ToString(CultureInfo.InvariantCulture)}) → {ParentValue?.Type?.Key ?? ParentTypeId.ToString(CultureInfo.InvariantCulture)} ({ParentValue?.Key ?? ParentValueId.ToString(CultureInfo.InvariantCulture)})";
}
