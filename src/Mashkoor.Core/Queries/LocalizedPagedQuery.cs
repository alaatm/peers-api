namespace Mashkoor.Core.Queries;

/// <summary>
/// Represents a localized paged query.
/// </summary>
/// <param name="Page">The page number.</param>
/// <param name="PageSize">The page size.</param>
/// <param name="SortField">The sort field.</param>
/// <param name="SortOrder">he sort order.</param>
/// <param name="Filters">The filters.</param>
public abstract record LocalizedPagedQuery(
    int? Page,
    int? PageSize,
    string? SortField,
    string? SortOrder,
    string? Filters) : LocalizedQuery();
