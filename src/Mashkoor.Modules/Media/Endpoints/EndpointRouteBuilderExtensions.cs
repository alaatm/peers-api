using Mashkoor.Modules.Media.Commands;
using Mashkoor.Modules.Media.Queries;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Mashkoor.Modules.Media.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapMediaEndpoints(this RouteGroupBuilder ep)
    {
        var gMedia = ep
            .MapGroup("/media")
            .WithTags("Media");

        gMedia.MapPost("/", (IMediator mediator, Upload.Command cmd)
            => mediator.Send(cmd))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<Accepted<Upload.BatchIdObj>>(StatusCodes.Status202Accepted);

        gMedia.MapGet("/", (int? page, int? pageSize, string? sortField, string? sortOrder, string? filters, IMediator mediator)
            => mediator.Send(new ListUploads.Query(page, pageSize, sortField, sortOrder, filters)))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<PagedQueryResponse<ListUploads.Response>>(StatusCodes.Status200OK);

        gMedia.MapGet("/status/", (Guid batchId, IMediator mediator)
            => mediator.Send(new GetStatus.Query(batchId)))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<Ok<GetStatus.Response>>(StatusCodes.Status200OK);

        return ep;
    }
}
