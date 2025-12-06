using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Peers.Core.Background;
using Peers.Core.Common.Configs;
using Peers.Core.Communication.Push;
using Peers.Core.Payments;
using Peers.Core.Payments.Providers.ClickPay.Configuration;
using Peers.Modules.Kernel;
using Peers.Modules.Kernel.Startup;

namespace Peers.Api.Test.EndToEnd;

public class ApiAppFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryDatabaseRoot _dbRoot = new();

    public Mock<IPushNotificationService> PushServiceMoq { get; } = new();
    public Mock<IPaymentProvider> PaymentProviderMoq { get; } = new(MockBehavior.Strict);

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

            var dbContextDescriptors = services.Where(d => d.ServiceType.ToString().Contains("PeersContext")).ToArray();
            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }
            services.AddDbContextPool<PeersContext>(options => options.UseInMemoryDatabase("GlobalMiddlewares.ApiAppFactory", _dbRoot));

            services.Replace(new ServiceDescriptor(typeof(IPushNotificationService), PushServiceMoq.Object));
            services.Replace(new ServiceDescriptor(typeof(IPaymentProvider), PaymentProviderMoq.Object));

            services.AddSingleton(new ClickPayConfig());
        });
}

[CollectionDefinition("Api App Factory collection")]
public class ApiAppFactoryCollection : ICollectionFixture<ApiAppFactory>
{
}
