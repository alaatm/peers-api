using Mashkoor.Core.Data;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Users.Queries;

public static class ListPushNotificationProblems
{
    [Authorize(Roles = Roles.UsersManager)]
    public sealed record Query(
        int? Page,
        int? PageSize,
        string? SortField,
        string? SortOrder,
        string? Filters) : PagedQuery(Page, PageSize, SortField, SortOrder, Filters);

    public sealed class Handler : ICommandHandler<Query>
    {
        private readonly MashkoorContext _context;

        public Handler(MashkoorContext context) => _context = context;

        public async Task<IResult> Handle([NotNull] Query cmd, CancellationToken ctk)
        {
            var q = _context
                .PushNotificationProblems
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .ApplyFilters(cmd.Filters)
                .ApplySorting(cmd.SortField, cmd.SortOrder);

            var data = await q.ApplyPaging(cmd.Page, cmd.PageSize).ToArrayAsync(ctk);
            var total = await q.CountAsync(ctk);
            return Result.Ok(new PagedQueryResponse<PushNotificationProblem>(data, total));
        }
    }
}
