using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mashkoor.Core.Common.Configs;

namespace Mashkoor.Core.Test.Common.Configs;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void RegisterConfig_registers_config_and_validator()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection([])
            .Build();

        // Act
        var services = new ServiceCollection()
            .RegisterConfig<TestConfig, TestConfigValidator>(config)
            .BuildServiceProvider();

        // Assert
        services.GetRequiredService<TestConfig>();
        services.GetRequiredService<IOptions<TestConfig>>();
        services.GetRequiredService<IValidateOptions<TestConfig>>();
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
