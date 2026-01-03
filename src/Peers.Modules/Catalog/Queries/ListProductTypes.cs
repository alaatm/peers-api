using Peers.Core.Data;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Translations;

namespace Peers.Modules.Catalog.Queries;

public static class ListProductTypes
{
    /// <summary>
    /// Represents a paged query for catalog items, including optional filtering and sorting parameters.
    /// </summary>
    /// <remarks>This query can only be executed by users with the CatalogManager or Seller roles. All
    /// parameters are optional; defaults will be applied if values are not provided.</remarks>
    /// <param name="Page">The zero-based index of the page to retrieve. Specify null to use the default page.</param>
    /// <param name="PageSize">The maximum number of items to include in a single page. Specify null to use the default page size.</param>
    /// <param name="SortField">The name of the field by which to sort the results. Specify null to use the default sort field.</param>
    /// <param name="SortOrder">The sort direction to apply to the results. Expected values are "asc" for ascending or "desc" for descending.
    /// Specify null to use the default sort order.</param>
    /// <param name="Filters">A filter expression to apply to the query results. Specify null to retrieve unfiltered results.</param>
    [Authorize(Roles = $"{Roles.CatalogManager}, {Roles.Seller}")]
    public sealed record Query(
        int? Page,
        int? PageSize,
        string? SortField,
        string? SortOrder,
        string? Filters) : PagedQuery(Page, PageSize, SortField, SortOrder, Filters);

    /// <summary>
    /// Represents a product type response, including identification, naming, hierarchy, and state information.
    /// </summary>
    /// <param name="Id">The unique identifier for the product type.</param>
    /// <param name="ParentId">The identifier of the parent product type, or <see langword="null"/> if this product type has no parent.</param>
    /// <param name="Names">An array of localized names for the product type. Must not be <see langword="null"/>.</param>
    /// <param name="SlugPath">The URL-friendly path segment representing the product type's location in the hierarchy. Must not be <see
    /// langword="null"/>.</param>
    /// <param name="Version">The version number of the product type, used for concurrency control.</param>
    /// <param name="State">The current state of the product type.</param>
    /// <param name="Kind">The kind of product type represented.</param>
    public sealed record Response(
        int Id,
        int? ParentId,
        ProductTypeTr.Dto[] Names,
        string SlugPath,
        int Version,
        ProductTypeState State,
        ProductTypeKind Kind)
    {
        public Response() : this(default, default, default!, default!, default, default, default) { }
    }

    public sealed class Handler : ICommandHandler<Query>
    {
        private readonly PeersContext _context;

        public Handler(PeersContext context)
            => _context = context;

        public async Task<IResult> Handle([NotNull] Query cmd, CancellationToken ctk)
        {
            var r = _context
                .ProductTypes
                .Select(p => new Response
                {
                    Id = p.Id,
                    ParentId = p.ParentId,
                    Names = p.Translations.Select(p => new ProductTypeTr.Dto { LangCode = p.LangCode, Name = p.Name }).ToArray(),
                    SlugPath = p.SlugPath,
                    Version = p.Version,
                    State = p.State,
                    Kind = p.Kind
                })
                .OrderBy(p => p.Id)
                .ApplyFilters(cmd.Filters)
                .ApplySorting(cmd.SortField, cmd.SortOrder);

            var data = await r.ApplyPaging(cmd.Page, cmd.PageSize).ToArrayAsync(ctk);
            var total = await r.CountAsync(ctk);
            return Result.Ok(new PagedQueryResponse<Response>(data, total));
        }
    }
}
