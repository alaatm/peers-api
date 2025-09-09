using Peers.Core.Data;
using Peers.Modules.Media.Domain;

namespace Peers.Modules.Media.Queries;

public static class ListUploads
{
    /// <summary>
    /// Lists uploaded media files.
    /// </summary>
    /// <param name="Page"></param>
    /// <param name="PageSize"></param>
    /// <param name="SortField"></param>
    /// <param name="SortOrder"></param>
    /// <param name="Filters"></param>
    [Authorize(Roles = Roles.Staff)]
    public sealed record Query(
        int? Page,
        int? PageSize,
        string? SortField,
        string? SortOrder,
        string? Filters) : PagedQuery(Page, PageSize, SortField, SortOrder, Filters);

    public sealed record Response(
        KeyValuePair<int, string> UploadedBy,
        Uri Url,
        string? Description,
        MediaType Type,
        MediaCategory Category,
        UploadStatus Status);

    public sealed class Handler : ICommandHandler<Query>
    {
        private readonly PeersContext _context;

        public Handler(PeersContext context) => _context = context;

        public async Task<IResult> Handle([NotNull] Query cmd, CancellationToken ctk)
        {
            var q = _context
                .MediaFiles
                .Where(p => p.Original == null)
                .OrderBy(p => p.Id)
                .Select(p => new Response(
                    new(p.CustomerId, p.Customer.Username),
                    p.MediaUrl,
                    p.Description,
                    p.Type,
                    p.Category,
                    p.Status
                ))
                .ApplyFilters(cmd.Filters)
                .ApplySorting(cmd.SortField, cmd.SortOrder);

            var data = await q.ApplyPaging(cmd.Page, cmd.PageSize).ToArrayAsync(ctk);
            var total = await q.CountAsync(ctk);
            return Result.Ok(new PagedQueryResponse<Response>(data, total));
        }
    }
}
