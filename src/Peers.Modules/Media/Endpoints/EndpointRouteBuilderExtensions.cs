using Peers.Modules.Media.Commands;
using Peers.Modules.Media.Queries;

namespace Peers.Modules.Media.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapMediaEndpoints(this RouteGroupBuilder ep)
    {
        var gMedia = ep
            .MapGroup("/media")
            .WithTags("Media");

        gMedia.MapPost("/", (IMediator mediator, Upload.Command cmd)
            => mediator.Send(cmd))
            .Accepts<Upload.CommandDoc>("multipart/form-data")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<Upload.BatchIdObj>(StatusCodes.Status202Accepted);

        gMedia.MapGet("/", (int? page, int? pageSize, string? sortField, string? sortOrder, string? filters, IMediator mediator)
            => mediator.Send(new ListUploads.Query(page, pageSize, sortField, sortOrder, filters)))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<PagedQueryResponse<ListUploads.Response>>(StatusCodes.Status200OK);

        gMedia.MapGet("/status/", (Guid batchId, IMediator mediator)
            => mediator.Send(new GetStatus.Query(batchId)))
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<GetStatus.Response>(StatusCodes.Status200OK);

        return ep;
    }
}
