using Peers.Modules.Customers.Commands;
using Peers.Modules.Media.Commands;

namespace Peers.Modules.Customers.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapCustomerEndpoints(this RouteGroupBuilder ep)
    {
        var gCustomers = ep
            .MapGroup("/customer")
            .WithTags("Customers");

        gCustomers.MapPost("/me/pin-code", (IMediator mediator, CreatePinCode.Command cmd)
            => mediator.Send(cmd))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status201Created);

        gCustomers.MapPut("/me/pin-code", (IMediator mediator, ChangePinCode.Command cmd)
            => mediator.Send(cmd))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status204NoContent);

        gCustomers.MapPut("/me/avatar", (IMediator mediator, SetProfilePicture.Command cmd)
            => mediator.Send(cmd))
            .Accepts<SetProfilePicture.CommandDoc>("multipart/form-data")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<Upload.BatchIdObj>(StatusCodes.Status202Accepted);

        return ep;
    }
}
