using System.Diagnostics.CodeAnalysis;

namespace Mashkoor.Core.Queries;

/// <summary>
/// Represents a paged query.
/// </summary>
/// <param name="Page">The page number.</param>
/// <param name="PageSize">The page size.</param>
/// <param name="SortField">The sort field.</param>
/// <param name="SortOrder">he sort order.</param>
/// <param name="Filters">The filters.</param>
[ExcludeFromCodeCoverage]
public abstract record PagedQuery(
    int? Page,
    int? PageSize,
    string? SortField,
    string? SortOrder,
    string? Filters) : IQuery;
