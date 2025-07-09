using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Mashkoor.Core.Commands;

namespace Mashkoor.Modules.Test;

public static class IActionResultExtensions
{
    public static int ExtractId(this IResult actionResult) => actionResult switch
    {
        Created<IdObj> created => created.Value.Id,
        Accepted<StatusObj> accepted => accepted.ExtractStatusId(),
        _ => throw new InvalidOperationException("Unknown created result value type."),
    };

    public static int ExtractStatusId(this IResult actionResult) => actionResult switch
    {
        Accepted<StatusObj> created => created.Value.StatusId,
        _ => throw new InvalidOperationException("Invalid action result type."),
    };
}
