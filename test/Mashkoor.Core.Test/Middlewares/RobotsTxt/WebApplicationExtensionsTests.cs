using Mashkoor.Core.Middlewares.RobotsTxt;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Mashkoor.Core.Test.Middlewares.RobotsTxt;

public class WebApplicationExtensionsTests
{
    [Fact]
    public async Task RobotsTxt_should_return_correct_response()
    {
        // Arrange
        using var host = await new HostBuilder()
            .ConfigureWebHost(p => p
                .UseTestServer()
                .Configure(app => app.UseRobotsTxt()))
            .StartAsync();

        // Act
        var response = await host.GetTestClient().GetAsync("/robots.txt");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("User-agent: *\nDisallow: /", content);
    }
}
