using Peers.Modules.Listings.Commands;

namespace Peers.Modules.Listings.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapListingsEndpoints(this RouteGroupBuilder ep)
    {
        var gListings = ep
            .MapGroup("/listings")
            .WithTags("Listings");

        gListings.MapPost("/", (IMediator mediator, CreateListing.Command cmd)
            => mediator.Send(cmd))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<CreateListing.Response>(StatusCodes.Status201Created);

        gListings.MapPost("/{id:int}/attributes", (IMediator mediator, int id, SetAttributes.Command cmd)
            => mediator.Send(cmd with { ListingId = id }))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var param = operation.Parameters?.First(p => p.Name == "id");
                param?.Description = "The ID of the listing to set attributes for.";
                return Task.CompletedTask;
            });

        return ep;
    }
}
