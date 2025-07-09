using Mashkoor.Modules.I18n.Queries;

namespace Mashkoor.Modules.I18n.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapI18nEndpoints(this RouteGroupBuilder ep)
    {
        var g = ep
            .MapGroup("/languages")
            .WithTags("I18n");

        g.MapGet("/", (IMediator mediator)
            => mediator.Send(new ListLanguages.Query()))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<string[]>(StatusCodes.Status200OK);

        return ep;
    }
}
