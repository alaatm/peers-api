namespace Mashkoor.Modules.I18n.Queries;

public static class ListLanguages
{
    [Authorize]
    public sealed record Query() : IQuery;

    public sealed class Handler : ICommandHandler<Query>
    {
        private readonly MashkoorContext _context;

        public Handler(MashkoorContext context) => _context = context;

        public async Task<IResult> Handle([NotNull] Query cmd, CancellationToken ctk)
        {
            var langCodes = await _context
                .Languages
                .OrderBy(p => p.Id)
                .Select(p => p.Id)
                .ToArrayAsync(ctk);

            return Result.Ok(langCodes);
        }
    }
}
