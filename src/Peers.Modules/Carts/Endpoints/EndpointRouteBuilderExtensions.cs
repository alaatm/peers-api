using Peers.Modules.Carts.Commands;
using Peers.Modules.Carts.Queries;

namespace Peers.Modules.Carts.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapCartEndpoints(this RouteGroupBuilder ep)
    {
        var gCart = ep
            .MapGroup("/cart")
            .WithTags("Cart");

        var gCheckout = ep
            .MapGroup("/checkout")
            .WithTags("Checkout");

        var gCheckoutSessions = ep
            .MapGroup("/checkout-sessions")
            .WithTags("CheckoutSessions");

        gCart.MapPost("/", (IMediator mediator, AddLineItem.Command cmd)
            => mediator.Send(cmd))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status200OK);

        gCart.MapPatch("/", (IMediator mediator, UpdateLineItemQuantity.Command cmd)
            => mediator.Send(cmd))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status200OK);

        gCart.MapDelete("/", (IMediator mediator, int listingId, string variantKey)
            => mediator.Send(new RemoveLineItem.Command(listingId, variantKey)))
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status200OK);

        gCheckout.MapPost("/preview", (IMediator mediator, Checkout.Command cmd)
            => mediator.Send(cmd))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<Checkout.Response>(StatusCodes.Status200OK);

        gCheckout.MapPost("/pay", (IMediator mediator, Pay.Command cmd)
            => mediator.Send(cmd))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<Pay.Response>(StatusCodes.Status202Accepted);

        gCheckoutSessions.MapGet("/{sessionId}", (IMediator mediator, Guid sessionId)
            => mediator.Send(new GetCheckoutSessionStatus.Query(sessionId)))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<GetCheckoutSessionStatus.Response>(StatusCodes.Status200OK)
            .WithName(GetCheckoutSessionStatus.EndpointName);

        return ep;
    }
}
