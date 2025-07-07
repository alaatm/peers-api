using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Mashkoor.Core.Test;

public static class TestHttpClient
{
    public static async Task<HttpClient> GetTestClientAsync(
        Action<IServiceCollection> servicesCfg = null,
        Action<IApplicationBuilder> appCfg = null,
        Action<IEndpointRouteBuilder> epCgf = null)
        => (await GetTestHostAsync(servicesCfg, appCfg, epCgf)).GetTestClient();

    public static Task<IHost> GetTestHostAsync(
        Action<IServiceCollection> servicesCfg = null,
        Action<IApplicationBuilder> appCfg = null,
        Action<IEndpointRouteBuilder> epCgf = null)
    {
        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(wh =>
            {
                wh.UseTestServer();
                wh.ConfigureServices(s =>
                {
                    s.AddRouting();
                    servicesCfg?.Invoke(s);
                });
                wh.Configure(app =>
                {
                    appCfg?.Invoke(app);
                    app.UseRouting();
                    app.UseEndpoints(ep => epCgf?.Invoke(ep));
                });
            });

        return hostBuilder.StartAsync();
    }
}
