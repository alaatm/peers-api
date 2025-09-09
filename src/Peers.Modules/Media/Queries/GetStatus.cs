using Peers.Modules.Media.Domain;

namespace Peers.Modules.Media.Queries;

public static class GetStatus
{
    /// <summary>
    /// Retrieves the status of a specific upload batch.
    /// </summary>
    /// <param name="BatchId">The upload batch id.</param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Query(Guid BatchId) : IQuery;

    public sealed record Response(Dictionary<Uri, UploadStatus> Status);

    public sealed class Handler : ICommandHandler<Query>
    {
        private readonly PeersContext _context;

        public Handler(PeersContext context)
            => _context = context;

        public async Task<IResult> Handle([NotNull] Query cmd, CancellationToken ctk)
        {
            var batch = await _context
                .MediaFiles
                .AsNoTracking()
                .Where(b => b.BatchId == cmd.BatchId)
                .Select(p => new
                {
                    p.MediaUrl,
                    p.Status
                })
                .ToDictionaryAsync(
                    p => p.MediaUrl,
                    p => p.Status,
                    ctk);

            return batch.Count == 0
                ? Results.NotFound()
                : Results.Ok(new Response(batch));
        }
    }
}
