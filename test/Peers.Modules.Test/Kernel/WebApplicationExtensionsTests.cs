using System.Net;
using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Peers.Modules.Test.Kernel;

public class WebApplicationExtensionsTestsBase
{
    [Fact]
    public void UsePeers_adds_dev_env_middleware_and_endpoints()
    {
        // Arrange
        var app = BuildApp(Environments.Development);

        // Act
        app.UsePeers();

        // Assert
        var middlewareList = GetMiddlewareList(app);
        Assert.Equal(6, middlewareList.Count);
        Assert.Equal("DefaultFilesMiddleware", middlewareList[0].Name);
        Assert.Equal("HttpsRedirectionMiddleware", middlewareList[1].Name);
        Assert.Contains("MapWhenExtension", middlewareList[2].FullName); // RobotsTxtMiddleware
        Assert.Contains("AuthenticationMiddleware", middlewareList[3].FullName);
        Assert.Equal("RateLimitingMiddleware", middlewareList[4].Name);
        Assert.Equal("RequestLocalizationMiddleware", middlewareList[5].Name);
    }

    [Fact]
    public void UsePeers_adds_prod_env_middleware_and_endpoints()
    {
        // Arrange
        var app = BuildApp(Environments.Production);

        // Act
        app.UsePeers();

        // Assert
        var middlewareList = GetMiddlewareList(app);
        Assert.Equal(7, middlewareList.Count);
        Assert.Equal("DefaultFilesMiddleware", middlewareList[0].Name);
        Assert.Contains("ExceptionHandlerExtensions", middlewareList[1].ToString());
        Assert.Equal("HttpsRedirectionMiddleware", middlewareList[2].Name);
        Assert.Contains("MapWhenExtension", middlewareList[3].FullName); // RobotsTxtMiddleware
        Assert.Contains("AuthenticationMiddleware", middlewareList[4].FullName);
        Assert.Equal("RateLimitingMiddleware", middlewareList[5].Name);
        Assert.Equal("RequestLocalizationMiddleware", middlewareList[6].Name);
    }

    private static WebApplication BuildApp(string environmentName)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = environmentName,
        });

        builder.Services.AddLocalization();
        builder.Services.AddAuthorization();
        builder.Services.AddMvcCore().AddRazorPages();
        builder.Services.AddRateLimiter(cfg
            => cfg.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(_
                => RateLimitPartition.GetTokenBucketLimiter(IPAddress.Any, _
                    => new TokenBucketRateLimiterOptions())));
        return builder.Build();
    }

    private static readonly PropertyInfo _piAppBuilder = typeof(WebApplication).GetProperty("ApplicationBuilder", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo _fiComponents = typeof(ApplicationBuilder).GetField("_components", BindingFlags.NonPublic | BindingFlags.Instance);

    protected static List<Type> GetMiddlewareList(WebApplication app)
    {
        var listOfMiddleware = new List<Type>();
        var appBuilder = _piAppBuilder.GetValue(app) as IApplicationBuilder;
        var components = _fiComponents.GetValue(appBuilder) as List<Func<RequestDelegate, RequestDelegate>>;
        foreach (var m in components)
        {
            var fiMiddleware = m.Target.GetType().GetField("_middleware", BindingFlags.NonPublic | BindingFlags.Instance);
            listOfMiddleware.Add(fiMiddleware is null
                ? m.Target.GetType()
                : fiMiddleware.GetValue(m.Target) as Type);
        }

        return listOfMiddleware;
    }
}
