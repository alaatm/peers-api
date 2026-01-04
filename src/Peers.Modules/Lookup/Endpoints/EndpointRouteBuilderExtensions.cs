using Peers.Modules.Lookup.Commands;

namespace Peers.Modules.Lookup.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapLookupTypesEndpoints(this RouteGroupBuilder ep)
    {
        var gLookupTypes = ep
            .MapGroup("/lookup-types")
            .WithTags("LookupTypes");

        gLookupTypes.MapPost("/", (IMediator mediator, CreateLookupType.Command cmd)
            => mediator.Send(cmd))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<IdObj>(StatusCodes.Status201Created);

        gLookupTypes.MapPost("/{key}/options", (string key, IMediator mediator, AddLookupOption.Command cmd)
            => mediator.Send(cmd with { LookupTypeKey = key }))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<IdObj>(StatusCodes.Status201Created)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var param = operation.Parameters?.First(p => p.Name == "key");
                param?.Description = "The key of the lookup type to which the new option will be added.";
                return Task.CompletedTask;
            });

        gLookupTypes.MapPost("/{key}/links", (string key, IMediator mediator, LinkLookupOptions.Command cmd)
            => mediator.Send(cmd with { LookupTypeKey = key }))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK)
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                var param = operation.Parameters?.First(p => p.Name == "key");
                param?.Description = "The key of the parent lookup type for which options will be linked.";
                return Task.CompletedTask;
            });

        return ep;
    }
}
