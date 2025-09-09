using System.Diagnostics.CodeAnalysis;

namespace Peers.Core.Commands;

#pragma warning disable CA1815 // Override equals and operator equals on value types
/// <summary>
/// Represents a created object that can be returned for 201 results.
/// </summary>
[ExcludeFromCodeCoverage]
public readonly struct IdObj
{
    /// <summary>
    /// The id.
    /// </summary>
    public int Id { get; }
    public IdObj(int id) => Id = id;
}

/// <summary>
/// Represents a created object with status id tracking that can be returned for 201 results.
/// </summary>
[ExcludeFromCodeCoverage]
public readonly struct StatusObj
{
    /// <summary>
    /// The status id.
    /// </summary>
    public int StatusId { get; }
    public StatusObj(int statusId) => StatusId = statusId;
}
#pragma warning restore CA1815 // Override equals and operator equals on value types
