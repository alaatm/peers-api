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
            => mediator.Send(cmd with { Id = id }))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var param = operation.Parameters?.First(p => p.Name == "id");
                param?.Description = "The ID of the listing to set attributes for.";
                return Task.CompletedTask;
            });

        gListings.MapPost("/{id:int}/variants/{sku}/logistics-profile", (IMediator mediator, int id, string sku, SetVariantLogisticsProfile.Command cmd)
            => mediator.Send(cmd with { Id = id, Sku = sku }))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var param1 = operation.Parameters?.First(p => p.Name == "id");
                param1?.Description = "The ID of the listing to which the variant belongs.";

                var param2 = operation.Parameters?.First(p => p.Name == "sku");
                param2?.Description = "The SKU of the listing variant to set the logistics profile for.";

                return Task.CompletedTask;
            });

        gListings.MapPost("/{id:int}/variants/logistics-profile", (IMediator mediator, int id, SetVariantLogisticsProfile.Command cmd)
            => mediator.Send(cmd with { Id = id }))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var param1 = operation.Parameters?.First(p => p.Name == "id");
                param1?.Description = "The ID of the listing to which the variants belongs.";

                return Task.CompletedTask;
            });

        gListings.MapPost("/{id:int}/publish", (IMediator mediator, int id, Publish.Command cmd)
            => mediator.Send(cmd with { Id = id }))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var param = operation.Parameters?.First(p => p.Name == "id");
                param?.Description = "The ID of the listing to publish.";
                return Task.CompletedTask;
            });

        return ep;
    }
}
