using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Peers.Core.Middlewares.RobotsTxt;

public static class WebApplicationExtensions
{
    private static readonly byte[] _response = Encoding.UTF8.GetBytes("User-agent: *\nDisallow: /");

    /// <summary>
    /// Adds robots txt middleware.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void UseRobotsTxt([NotNull] this IApplicationBuilder app) => app.MapWhen(
        context => context.Request.Path.StartsWithSegments("/robots.txt", StringComparison.OrdinalIgnoreCase),
        appBuilder => appBuilder.Run(async context =>
        {
            context.Response.ContentType = "text/plain";
            context.Response.ContentLength = _response.Length;
            await context.Response.Body.WriteAsync(_response);
        })
    );
}
