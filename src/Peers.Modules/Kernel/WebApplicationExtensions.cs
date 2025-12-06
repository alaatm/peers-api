using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Peers.Core.Localization;
using Peers.Core.Middlewares.RobotsTxt;
using Peers.Core.Nafath;
using Peers.Core.RateLimiting;
using Peers.Modules.Customers.Endpoints;
using Peers.Modules.I18n.Endpoints;
using Peers.Modules.Media.Endpoints;
using Peers.Modules.Sellers;
using Peers.Modules.Sellers.Endpoints;
using Peers.Modules.SystemInfo.Endpoints;
using Peers.Modules.Users.Endpoints;
using Scalar.AspNetCore;

namespace Peers.Modules.Kernel;

/// <summary>
/// Provides WebApplication extension methods.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Adds all required middleware.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void UsePeers([NotNull] this WebApplication app)
    {
        var badRequestResponse = Encoding.UTF8.GetBytes(/*lang=json,strict*/ "{\"detail\":\"Bad request.\"}");
        var serverErrorResponse = Encoding.UTF8.GetBytes(/*lang=json,strict*/ "{\"detail\":\"An error has occurred.\"}");

        app.UseDefaultFiles();
        app.MapStaticAssets();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(p => p.Title = "Peers API Reference");
        }
        else
        {
            app.UseExceptionHandler(cfg =>
                cfg.Run(async context =>
                {
                    if (context.Features.Get<IExceptionHandlerFeature>() is IExceptionHandlerFeature f &&
                        f.Error is BadHttpRequestException)
                    {
                        context.Response.StatusCode = 400;
                        context.Response.ContentType = "application/json";
                        await context.Response.Body.WriteAsync(badRequestResponse);
                    }
                    else
                    {
                        context.Response.ContentType = "application/json";
                        await context.Response.Body.WriteAsync(serverErrorResponse);
                    }
                }));
        }

        app.UseHttpsRedirection();
        app.MapFallbackToFile("/index.html");

        app.UseRobotsTxt();
        app.UseAuthentication();
        //app.UseOutputCache();
        app.UseRateLimiter();
        app.UseAuthorization();
        app.UseLocalization();

        var all = app.MapGroup("/api/v1");
        var protectedGroup = all.MapGroup("");
        var publicGroup = all.MapGroup("");

        protectedGroup
            .MapAccountEndpoints()
            .MapI18nEndpoints()
            .MapCustomerEndpoints()
            .MapSellerEndpoints()
            .MapMediaEndpoints()
            .MapSystemEndpoints()
            .RequireRateLimiting(GenericRateLimiter.PerUserRateLimitPolicyName)
            .WithTags("Protected");

        publicGroup
            .MapPublicAccountEndpoints()
            .MapNafathCallbackEndpoint(NafathCallback.Handler)
            .WithTags("Public");

        app.MapRazorPages();

        app.MapGet("/api/v1/throw-test", (ctx) => throw new InvalidOperationException("I always throw")).ExcludeFromDescription();
        app.MapPost("/api/v1/test", (InvalidOperationException _) => Results.Ok()).ExcludeFromDescription();
        // This is to prevent 404 from AlwaysOn pings
        app.MapGet("/", () => Results.Ok()).ExcludeFromDescription();
    }
}
