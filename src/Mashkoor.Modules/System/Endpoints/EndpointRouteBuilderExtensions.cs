using Mashkoor.Modules.System.Queries;

namespace Mashkoor.Modules.System.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapSystemEndpoints(this RouteGroupBuilder ep)
    {
        var g = ep
            .MapGroup("/system")
            .WithTags("System");

        g.MapGet("/langs", (IMediator mediator)
            => mediator.Send(new ListSupportedLanguages.Query()))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<PagedQueryResponse<ListSupportedLanguages.Response>>(StatusCodes.Status200OK);

        return ep;
    }
}
