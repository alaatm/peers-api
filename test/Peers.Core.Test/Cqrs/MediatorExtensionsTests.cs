//using MediatR;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Peers.Core.Commands;
//using Peers.Core.Cqrs;
//using Peers.Core.Http;

//namespace Peers.Core.Test.Cqrs;

//public class MediatorExtensionsTests
//{
//    [Fact]
//    public async Task SendAsync_throws_for_non_supported_result_type()
//    {
//        // Arrange
//        var unsupportedResult = new JsonResult("");
//        var mediatorMoq = new Mock<IMediator>(MockBehavior.Strict);
//        mediatorMoq.Setup(p => p.Send(It.IsAny<ICommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(unsupportedResult).Verifiable();

//        // Act and assert
//        var ex = await Assert.ThrowsAnyAsync<InvalidOperationException>(() => mediatorMoq.Object.SendAsync(Mock.Of<ICommand>()));
//        Assert.Equal($"Result of type {unsupportedResult.GetType()} could not be mapped to IResult.", ex.Message);
//    }

//    [Theory]
//    [MemberData(nameof(SendAsync_TestData))]
//    public async Task SendAsync_converts_IActionResult_to_IResult(IResult result, IResult expectedConvertedResult)
//    {
//        // Arrange
//        var mediatorMoq = new Mock<IMediator>(MockBehavior.Strict);
//        mediatorMoq.Setup(p => p.Send(It.IsAny<ICommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(result).Verifiable();

//        // Act
//        var convertedResult = await mediatorMoq.Object.SendAsync(Mock.Of<ICommand>());

//        // Assert
//        Assert.IsType(expectedConvertedResult.GetType(), convertedResult);
//    }

//    public static TheoryData<IResult, IResult> SendAsync_TestData() => new()
//    {
//        { new AcceptedResult(), Results.Accepted() },
//        { new BadRequest<ProblemDetails>("value"), Results.BadRequest("value") },
//        { new ConflictObjectResult("value"), Results.Conflict("value") },
//        { new Created<IdObj>("location", "value"), Results.Created("location", "value") },
//        { new ForbiddenActionResult("value"), new ForbiddenHttpObjectResult("value") },
//        { new NoContent(), Results.NoContent() },
//        { new NotFoundResult(), Results.NotFound() },
//        { new OkObjectResult("value"), Results.Ok("value") },
//        { new UnauthorizedHttpResult<ProblemDetails>("value"), new UnauthorizedHttpObjectResult("value") },
//        { new ObjectResult(new HttpValidationProblemDetails()), Results.ValidationProblem(new Dictionary<string, string[]>()) },
//        { new ObjectResult(new ProblemDetails()), Results.Problem() },
//    };
//}
