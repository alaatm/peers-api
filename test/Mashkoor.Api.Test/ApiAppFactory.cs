using Mashkoor.Core.Background;
using Mashkoor.Modules.Kernel;
using Mashkoor.Modules.Kernel.Startup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Mashkoor.Api.Test.GlobalMiddlewares;

public class ApiAppFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryDatabaseRoot _dbRoot = new();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder
            .ConfigureWebHost(p => p.UseConfiguration(TestConfig.Configuration))
            .UseEnvironment(Environments.Production);

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.ConfigureServices(services =>
        {
            var backgroundDescriptors = services.Where(d => d.ServiceType == typeof(IHostedService)).ToArray();
            // Remove message broker
            services.Remove(backgroundDescriptors.SingleOrDefault(d => d.ImplementationType == typeof(MessageBroker)));
            // Remove StartupBackgroundService startup task
            services.Remove(backgroundDescriptors.SingleOrDefault(d => d.ImplementationType == typeof(StartupBackgroundService)));

            var dbContextDescriptors = services.Where(d => d.ServiceType.ToString().Contains("MashkoorContext")).ToArray();
            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }
            services.AddDbContextPool<MashkoorContext>(options => options.UseInMemoryDatabase("GlobalMiddlewares.ApiAppFactory", _dbRoot));
        });
}

[CollectionDefinition("Api App Factory collection")]
public class ApiAppFactoryCollection : ICollectionFixture<ApiAppFactory>
{
}
