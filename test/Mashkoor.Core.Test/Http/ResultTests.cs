using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Mashkoor.Core.Http;

namespace Mashkoor.Core.Test.Http;

public class ResultTests
{
    [Fact] public void Forbidden_returns_expected_result() => Assert.IsType<ForbiddenHttpResult<ProblemDetails>>(Result.Forbidden());
    [Fact] public void AccessRestricted_returns_expected_result() => Assert.IsType<ForbiddenHttpResult<ProblemDetails>>(Result.AccessRestricted());
    [Fact] public void NotFound_returns_expected_result() => Assert.IsType<NotFound>(Result.NotFound());
    [Fact] public void File_returns_expected_result() => Assert.IsType<FileContentHttpResult>(Result.File([], "", ""));
    [Fact] public void Unauthorized_returns_expected_result() => Assert.IsType<UnauthorizedHttpResult2<ProblemDetails>>(Result.Unauthorized(""));
    [Fact] public void BadRequest_returns_expected_result() => Assert.IsType<BadRequest<ProblemDetails>>(Result.BadRequest("error"));
    [Fact] public void Conflict_returns_expected_result() => Assert.IsType<Conflict<ProblemDetails>>(Result.Conflict("error"));
    [Fact] public void NoContent_returns_expected_result() => Assert.IsType<NoContent>(Result.NoContent());
    [Fact] public void Ok_returns_expected_result() => Assert.IsType<Ok>(Result.Ok());
    [Fact] public void Problem_returns_expected_result() => Assert.IsType<ProblemHttpResult>(Result.Problem());
    [Fact] public void ValidationProblem_returns_expected_result() => Assert.IsType<ProblemHttpResult>(Result.ValidationProblem(new Dictionary<string, string[]>() { { "k", ["v"] } }));
    [Fact] public void Created_returns_expected_result() => Assert.IsType<Created<string>>(Result.Created(value: ""));
    [Fact] public void Accepted_returns_expected_result() => Assert.IsType<Accepted>(Result.Accepted());
}
