using Mashkoor.Modules.I18n.Domain;

namespace Mashkoor.Modules.System.Queries;

public static class ListSupportedLanguages
{
    [Authorize]
    public sealed record Query() : IQuery;

    public sealed record Response(
        string Name,
        string Code,
        string Dir)
    {
        public static Response[] FromSysLanguages()
        {
            var result = new Response[Language.SupportedLanguages.Length];

            for (var i = 0; i < Language.SupportedLanguages.Length; i++)
            {
                var lang = Language.SupportedLanguages[i];
                result[i] = new Response(lang.Name, lang.Id, lang.Dir);
            }

            return result;
        }
    }

    public sealed class Handler : ICommandHandler<Query>
    {
        public Handler() { }

        public Task<IResult> Handle([NotNull] Query cmd, CancellationToken ctk)
        {
            var result = Response.FromSysLanguages();
            return Task.FromResult(Result.Ok(new PagedQueryResponse<Response>(result, result.Length)));
        }
    }
}
