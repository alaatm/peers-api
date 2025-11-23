using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Peers.Core.Payments;
using Peers.Core.Payments.Configuration;
using Peers.Core.Payments.Providers.ClickPay;
using Peers.Core.Payments.Providers.ClickPay.Configuration;
using Peers.Core.Payments.Providers.Moyasar;
using Peers.Core.Payments.Providers.Moyasar.Configuration;

namespace Peers.Core.Test.Payments;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPaymentGateway_adds_required_services()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "paymentGateway:provider", "Moyasar" },
                { "ClickPayPaymentProvider:profileId", "123" },
                { "ClickPayPaymentProvider:key", "123" },
                { "ClickPayPaymentProvider:payoutAccountId", "123" },
                { "MoyasarPaymentProvider:publishableKey", "123" },
                { "MoyasarPaymentProvider:key", "123" },
                { "MoyasarPaymentProvider:payoutAccountId", "123" },
            })
            .Build();

        // Act
        var serviceProvider = new ServiceCollection()
            .AddPaymentGateway(config)
            .BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<PaymentGatewayConfig>();

        serviceProvider.GetRequiredService<IValidateOptions<ClickPayConfig>>();
        serviceProvider.GetRequiredService<ClickPayConfig>();

        serviceProvider.GetRequiredService<IValidateOptions<MoyasarConfig>>();
        serviceProvider.GetRequiredService<MoyasarConfig>();
    }

    [Theory]
    [InlineData(PaymentProvider.ClickPay)]
    [InlineData(PaymentProvider.Moyasar)]
    public void AddPaymentGateway_should_resolve_the_configured_provider(PaymentProvider paymentProvider)
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "paymentGateway:provider", paymentProvider.ToString() },
                { "ClickPayPaymentProvider:profileId", "123" },
                { "ClickPayPaymentProvider:key", "123" },
                { "ClickPayPaymentProvider:payoutAccountId", "123" },
                { "MoyasarPaymentProvider:publishableKey", "123" },
                { "MoyasarPaymentProvider:key", "123" },
                { "MoyasarPaymentProvider:payoutAccountId", "123" },
            })
            .Build();

        // Act
        var serviceProvider = new ServiceCollection()
            .AddPaymentGateway(config)
            .BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<IPaymentProvider>();

        // Assert
        Assert.NotNull(provider);
        if (paymentProvider == PaymentProvider.ClickPay)
        {
            Assert.IsType<ClickPayPaymentProvider>(provider);
        }
        else if (paymentProvider == PaymentProvider.Moyasar)
        {
            Assert.IsType<MoyasarPaymentProvider>(provider);
        }
    }

    [Fact]
    public void AddPaymentGateway_should_throw_when_resolving_unknown_provider()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "paymentGateway:provider", "99" },
            })
            .Build();

        // Act & assert
        var serviceProvider = new ServiceCollection()
            .AddPaymentGateway(config)
            .BuildServiceProvider();

        var ex = Assert.Throws<NotSupportedException>(serviceProvider.GetRequiredService<IPaymentProvider>);
        Assert.Equal("Payment provider 99 is not supported.", ex.Message);
    }
}
