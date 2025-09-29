namespace Peers.Modules.SystemInfo.Queries;

public static class GetClientApp
{
    [Authorize(Roles = Roles.Staff)]
    public sealed record Query : IQuery;

    public sealed record Response(
        string PackageName,
        string HashString,
        string AndroidStoreLink,
        string IosStoreLink,
        string VersionString);

    public sealed class Handler : ICommandHandler<Query>
    {
        private readonly PeersContext _context;

        public Handler(PeersContext context)
            => _context = context;

        public async Task<IResult> Handle([NotNull] Query cmd, CancellationToken ctk)
        {
            // TODO: Fix when this is resolved: https://github.com/dotnet/efcore/issues/36761
            var clientApp = await _context.ClientApps
                .AsNoTracking()
                .Select(c => new
                {
                    c.PackageName,
                    c.HashString,
                    c.AndroidStoreLink,
                    c.IOSStoreLink,
                    c.LatestVersion,
                })
                .FirstAsync(ctk);

            return Results.Ok(new Response(
                clientApp.PackageName,
                clientApp.HashString,
                clientApp.AndroidStoreLink,
                clientApp.IOSStoreLink,
                clientApp.LatestVersion.VersionString));
        }
    }
}
