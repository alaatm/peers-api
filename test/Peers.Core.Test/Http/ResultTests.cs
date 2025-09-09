using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Peers.Core.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Peers.Core.Test.Http;

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
    [Fact] public void Created_returns_expected_result() => Assert.IsType<Created>(Result.Created());
    [Fact] public void Created_typedResult_returns_expected_result() => Assert.IsType<Created<string>>(Result.Created(value: ""));
    [Fact] public void Accepted_returns_expected_result() => Assert.IsType<Accepted>(Result.Accepted());
    [Fact] public void Accepted_typedResult_returns_expected_result() => Assert.IsType<Accepted<int>>(Result.Accepted(null, 5));

    [Fact]
    public async Task ForbiddenHttpResult_executes_with_correct_status_code()
    {
        // Arrange
        var result = Result.Forbidden();
        var context = new DefaultHttpContext() { Response = { Body = new MemoryStream() } };
        context.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        // Act
        await result.ExecuteAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        Assert.Contains(/*lang=json,strict*/ "{\"type\":\"https://tools.ietf.org/html/rfc9110#section-15.5.4\",\"title\":\"Forbidden\",\"status\":403}", responseBody);
    }

    [Fact]
    public void ForbiddenHttpResult_sets_value_and_statusCode()
    {
        // Arrange
        var result = new ForbiddenHttpResult<int>(5);

        // Act & assert
        Assert.Equal(5, ((IValueHttpResult)result).Value);
        Assert.Equal(StatusCodes.Status403Forbidden, ((IStatusCodeHttpResult)result).StatusCode);
    }

    [Fact]
    public async Task UnauthorizedHttpResult2_executes_with_correct_status_code()
    {
        // Arrange
        var result = Result.Unauthorized();
        var context = new DefaultHttpContext() { Response = { Body = new MemoryStream() } };
        context.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        // Act
        await result.ExecuteAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        Assert.Contains(/*lang=json,strict*/ "{\"type\":\"https://tools.ietf.org/html/rfc9110#section-15.5.2\",\"title\":\"Unauthorized\",\"status\":401}", responseBody);
    }

    [Fact]
    public void UnauthorizedHttpResult2_sets_value_and_statusCode()
    {
        // Arrange
        var result = new UnauthorizedHttpResult2<int>(5);

        // Act & assert
        Assert.Equal(5, ((IValueHttpResult)result).Value);
        Assert.Equal(StatusCodes.Status401Unauthorized, ((IStatusCodeHttpResult)result).StatusCode);
    }

    [Fact]
    public async Task HttpResultsHelper_WriteResultAsJsonAsync_noops_when_value_is_null()
    {
        // Arrange
        var context = new DefaultHttpContext() { Response = { Body = new MemoryStream() } };

        // Act
        await HttpResultsHelper.WriteResultAsJsonAsync<string>(context, Mock.Of<ILogger>(), null);

        // Assert
        Assert.Null(context.Response.ContentType);
        Assert.Equal(0, context.Response.Body.Length);
    }

    [Fact]
    public async Task HttpResultsHelper_WriteResultAsJsonAsync_writes_valueType()
    {
        // Arrange
        var context = new DefaultHttpContext() { Response = { Body = new MemoryStream() } };

        // Act
        await HttpResultsHelper.WriteResultAsJsonAsync(context, Mock.Of<ILogger>(), 5);

        // Assert
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        Assert.Contains("5", responseBody);
    }

    [Fact]
    public async Task HttpResultsHelper_WriteResultAsJsonAsync_writes_json_result()
    {
        // Arrange
        var context = new DefaultHttpContext() { Response = { Body = new MemoryStream() } };

        // Act
        await HttpResultsHelper.WriteResultAsJsonAsync(context, Mock.Of<ILogger>(), new { message = "Hello, World!" });

        // Assert
        Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        Assert.Contains(/*lang=json,strict*/ "{\"message\":\"Hello, World!\"}", responseBody);
    }

    [Fact]
    public void ProblemDetailsDefaults_Apply_correctly_sets_statusCode_when_not_specified()
    {
        // Arrange
        var problem1 = new ProblemDetails();
        var problem2 = new HttpValidationProblemDetails();

        // Act
        ProblemDetailsDefaults.Apply(problem1, null);
        ProblemDetailsDefaults.Apply(problem2, null);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, problem1.Status);
        Assert.Equal(StatusCodes.Status400BadRequest, problem2.Status);
    }

    [Fact]
    public void ProblemDetailsDefaults_Apply_correctly_sets_title_when_statusCode_is_not_defined_in_defaults_and_title_is_not_set()
    {
        // Arrange
        var problem = new ProblemDetails();

        // Act
        ProblemDetailsDefaults.Apply(problem, 511);

        // Assert
        Assert.Equal("Network Authentication Required", problem.Title);
    }

    [Fact]
    public void ProblemDetailsDefaults_Apply_noops_when_statusCode_is_not_defined_in_defaults_and_title_is_set()
    {
        // Arrange
        var problem = new ProblemDetails() { Title = "Custom Title" };

        // Act
        ProblemDetailsDefaults.Apply(problem, 511);

        // Assert
        Assert.Equal("Custom Title", problem.Title);
    }
}
