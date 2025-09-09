using System.Diagnostics.CodeAnalysis;

namespace Peers.Core.Queries;

/// <summary>
/// Represents a paged query response.
/// </summary>
/// <typeparam name="T">The type of data.</typeparam>
/// <param name="Data">The data.</param>
/// <param name="Total">The total record count.</param>
[ExcludeFromCodeCoverage]
public record PagedQueryResponse<T>(
    T[] Data,
    int Total);
