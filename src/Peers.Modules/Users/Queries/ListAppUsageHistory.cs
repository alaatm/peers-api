using Peers.Core.Data;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Users.Queries;

public static class ListAppUsageHistory
{
    [Authorize(Roles = $"{Roles.UsersManager}")]
    public sealed record Query(
        int Id,
        int? Page,
        int? PageSize,
        string? SortField,
        string? SortOrder,
        string? Filters) : PagedQuery(Page, PageSize, SortField, SortOrder, Filters);

    public sealed class Handler : ICommandHandler<Query>
    {
        private readonly PeersContext _context;

        public Handler(PeersContext context) => _context = context;

        public async Task<IResult> Handle([NotNull] Query cmd, CancellationToken ctk)
        {
            var r = _context
                .Users
                .AsNoTracking()
                .Where(p => p.Id == cmd.Id)
                .SelectMany(p => p.AppUsage)
                .OrderByDescending(p => p.OpenedAt)
                .ApplyFilters(cmd.Filters)
                .ApplySorting(cmd.SortField, cmd.SortOrder);

            var data = await r.ApplyPaging(cmd.Page, cmd.PageSize).ToArrayAsync(ctk);
            var total = await r.CountAsync(ctk);
            return Result.Ok(new PagedQueryResponse<AppUsageHistory>(data, total));
        }
    }
}
