using Peers.Modules.Catalog.Commands;
using Peers.Modules.Catalog.Queries;

namespace Peers.Modules.Catalog.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapCatalogEndpoints(this RouteGroupBuilder ep)
    {
        var gCatalog = ep
            .MapGroup("/catalog")
            .WithTags("Catalog");

        gCatalog.MapPost("/", (IMediator mediator, CreateRoot.Command cmd)
            => mediator.Send(cmd))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status201Created);

        gCatalog.MapPost("/{id:int}/clone", (IMediator mediator, int id, Clone.Command cmd)
            => mediator.Send(cmd with { Id = id }))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status201Created)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var param = operation.Parameters?.First(p => p.Name == "id");
                param?.Description = "The ID of the product type to clone.";
                return Task.CompletedTask;
            });

        gCatalog.MapPost("/{id:int}", (IMediator mediator, int id, AddChild.Command cmd)
            => mediator.Send(cmd with { ParentId = id }))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status201Created)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var param = operation.Parameters?.First(p => p.Name == "id");
                param?.Description = "The ID of the parent product type under which the new child type will be created.";
                return Task.CompletedTask;
            });

        gCatalog.MapPost("/{id:int}/publish", (IMediator mediator, int id, Publish.Command cmd)
            => mediator.Send(cmd with { Id = id }))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var param = operation.Parameters?.First(p => p.Name == "id");
                param?.Description = "The ID of the product type to publish.";
                return Task.CompletedTask;
            });

        gCatalog.MapPost("/{id:int}/attributes", (IMediator mediator, int id, DefineAttribute.Command cmd)
            => mediator.Send(cmd with { Id = id }))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var param = operation.Parameters?.First(p => p.Name == "id");
                param?.Description = "The ID of the product type to define attributes for.";
                return Task.CompletedTask;
            });

        gCatalog.MapPost("/{id:int}/attributes/{key}/options", (IMediator mediator, int id, string key, AddEnumAttributeOption.Command cmd)
            => mediator.Send(cmd with { Id = id, Key = key }))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var param1 = operation.Parameters?.First(p => p.Name == "id");
                param1?.Description = "The ID of the product type to which the attribute belongs.";

                var param2 = operation.Parameters?.First(p => p.Name == "key");
                param2?.Description = "The key of the enum attribute definition to which the option will be added.";

                return Task.CompletedTask;
            });

        gCatalog.MapPost("/{id:int}/attributes/{key}/members", (IMediator mediator, int id, string key, AddGroupAttributeMember.Command cmd)
            => mediator.Send(cmd with { Id = id, Key = key }))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var param1 = operation.Parameters?.First(p => p.Name == "id");
                param1?.Description = "The ID of the product type to which the attribute belongs.";

                var param2 = operation.Parameters?.First(p => p.Name == "key");
                param2?.Description = "The key of the group attribute definition to which the member will be added.";

                return Task.CompletedTask;
            });

        gCatalog.MapGet("/", (int? page, int? pageSize, string? sortField, string? sortOrder, string? filters, IMediator mediator)
            => mediator.Send(new ListProductTypes.Query(page, pageSize, sortField, sortOrder, filters)))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<PagedQueryResponse<ListProductTypes.Response>>(StatusCodes.Status200OK);

        gCatalog.MapGet("/{id:int}", (IMediator mediator, int id)
            => mediator.Send(new GetProductTypeDetails.Query(id)))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<GetProductTypeDetails.Response>(StatusCodes.Status200OK)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var param = operation.Parameters?.First(p => p.Name == "id");
                param?.Description = "The ID of the product type to get the details for.";
                return Task.CompletedTask;
            });

        return ep;
    }
}
