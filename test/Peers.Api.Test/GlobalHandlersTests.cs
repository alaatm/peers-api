using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Peers.Api.Test.EndToEnd;

namespace Peers.Api.Test.GlobalMiddlewares;

[Collection("Api App Factory collection")]
public class GlobalHandlersTests : IClassFixture<ApiAppFactory>
{
    private readonly HttpClient _client;

    public GlobalHandlersTests(ApiAppFactory factory)
        => _client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

    [Fact]
    public async Task Handles_BadRequest_exceptions()
    {
        // Arrange
        var malformedContent = new StringContent("{,}", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/test", malformedContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(400, problem.Status);
        Assert.Equal("Bad request.", problem.Detail);
    }

    [Fact]
    public async Task Handles_Unhandled_exceptions()
    {
        // Arrange & act
        var response = await _client.GetAsync("/api/v1/throw-test");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(500, problem.Status);
        Assert.Equal("An error has occurred.", problem.Detail);
    }
}
