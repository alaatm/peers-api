using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mashkoor.Core.Common.Configs;
using Mashkoor.Core.Common.HttpClients;

namespace Mashkoor.Core.Test.Common.HttpClients;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void RegisterHttpClient_registers_client()
    {
        // Arrange & act
        var services = new ServiceCollection()
            .RegisterHttpClient<TestClient>()
            .BuildServiceProvider();

        // Assert
        services.GetRequiredService<TestClient>();
    }

    [Fact]
    public void RegisterHttpClient_registers_implementationClient()
    {
        // Arrange & act
        var services = new ServiceCollection()
            .RegisterHttpClient<ITestClient, TestClient>()
            .BuildServiceProvider();

        // Assert
        services.GetRequiredService<ITestClient>();
    }

    [Fact]
    public void RegisterHttpClient_registers_client_along_its_config()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection([])
            .Build();

        // Act
        var services = new ServiceCollection()
            .RegisterHttpClient<ITestClient, TestClient, TestConfig, TestConfigValidator>(config)
            .BuildServiceProvider();

        // Assert
        services.GetRequiredService<TestConfig>();
        services.GetRequiredService<IOptions<TestConfig>>();
        services.GetRequiredService<IValidateOptions<TestConfig>>();
        services.GetRequiredService<ITestClient>();
    }

    private class TestClient : ITestClient
    {
        public HttpClient HttpClient { get; }
        public TestClient(HttpClient httpClient) => HttpClient = httpClient;
    }
    private interface ITestClient
    {
        HttpClient HttpClient { get; }
    }

    private class TestConfig : IConfigSection
    {
        static string IConfigSection.ConfigSection => "test";
    }
    private class TestConfigValidator : IValidateOptions<TestConfig>
    {
        public ValidateOptionsResult Validate(string name, TestConfig options) => ValidateOptionsResult.Success;
    }
}
