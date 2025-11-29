
using Peers.Modules.Sellers.Commands;

namespace Peers.Modules.Sellers.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapSellerEndpoints(this RouteGroupBuilder ep)
    {
        var gSellers = ep
            .MapGroup("/sellers")
            .WithTags("Sellers");

        gSellers.MapPost("/enroll", (IMediator mediator, EnrollSeller.Command cmd)
            => mediator.Send(cmd))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<EnrollSeller.Response>(StatusCodes.Status200OK);

        return ep;
    }
}
