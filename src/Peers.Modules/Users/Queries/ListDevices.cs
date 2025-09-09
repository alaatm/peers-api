using Peers.Core.Data;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Users.Queries;

public static class ListDevices
{
    [Authorize(Roles = $"{Roles.UsersManager}")]
    public sealed record Query(
        int Id,
        int? Page,
        int? PageSize,
        string? SortField,
        string? SortOrder,
        string? Filters) : PagedQuery(Page, PageSize, SortField, SortOrder, Filters);

    public sealed record Response(
        int Id,
        Guid DeviceId,
        string Manufacturer,
        string Model,
        string Platform,
        string OSVersion,
        string Idiom,
        string DeviceType,
        string? Handle,
        DateTime RegisteredOn,
        string AppVersion,
        bool IsStalled,
        DateTime? LastPing,
        int Faults)
    {
        public Response() : this(default, default!, default!, default!, default!, default!, default!, default!, default!, default, default!, default, default, default) { }
    }

    public sealed class Handler : ICommandHandler<Query>
    {
        private readonly PeersContext _context;
        private readonly TimeProvider _timeProvider;

        public Handler(
            PeersContext context,
            TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }

        public async Task<IResult> Handle([NotNull] Query cmd, CancellationToken ctk)
        {
            var r = _context
                .Set<Device>()
                .Where(p => p.UserId == cmd.Id)
                .Select(p => new Response
                {
                    Id = p.Id,
                    DeviceId = p.DeviceId,
                    Manufacturer = p.Manufacturer,
                    Model = p.Model,
                    Platform = p.Platform,
                    OSVersion = p.OSVersion,
                    Idiom = p.Idiom,
                    DeviceType = p.DeviceType,
                    Handle = p.PnsHandle,
                    RegisteredOn = p.RegisteredOn,
                    AppVersion = p.AppVersion,
                    IsStalled = p.IsStalled(_timeProvider.UtcNow()),
                    LastPing = p.LastPing,
                    Faults = _context.Set<DeviceError>().Count(de => de.DeviceId == p.DeviceId),
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
