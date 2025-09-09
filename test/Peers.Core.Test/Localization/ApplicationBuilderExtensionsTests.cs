using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Peers.Core.Localization;
using static Peers.Core.Test.TestHttpClient;

namespace Peers.Core.Test.Localization;

public class ApplicationBuilderExtensionsTests
{
    [Fact]
    public async Task UseLocalization_adds_request_localization_services()
    {
        // Arrange
        string culture = null;

        var host = await GetTestHostAsync(
            appCfg: app => app.UseLocalization(),
            epCgf: ep => ep.MapGet("/", () => culture = Thread.CurrentThread.CurrentCulture.Name));

        // Act & assert
        var client = host.GetTestClient();
        client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("ar-SA"));
        await client.GetAsync("/");
        Assert.Equal("ar", culture);

        // Act & assert
        client = host.GetTestClient();
        await client.GetAsync("/");
        Assert.Equal("en", culture);
    }
}
