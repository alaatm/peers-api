namespace Mashkoor.Modules.System.Queries;

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
        private readonly MashkoorContext _context;

        public Handler(MashkoorContext context)
            => _context = context;

        public async Task<IResult> Handle([NotNull] Query cmd, CancellationToken ctk)
        {
            var clientApp = await _context.ClientApps
                .AsNoTracking()
                .Select(c => new Response(
                    c.PackageName,
                    c.HashString,
                    c.AndroidStoreLink,
                    c.IOSStoreLink,
                    c.LatestVersion.VersionString))
                .FirstAsync(ctk);

            return Results.Ok(clientApp);
        }
    }
}
