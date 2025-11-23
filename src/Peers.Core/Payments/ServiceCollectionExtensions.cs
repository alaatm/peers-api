using System.Diagnostics.CodeAnalysis;
using Peers.Core.Common.Configs;
using Peers.Core.Common.HttpClients;
using Peers.Core.Payments.Configuration;
using Peers.Core.Payments.Providers.ClickPay;
using Peers.Core.Payments.Providers.ClickPay.Configuration;
using Peers.Core.Payments.Providers.Moyasar;
using Peers.Core.Payments.Providers.Moyasar.Configuration;

namespace Peers.Core.Payments;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds payment-gateway services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The builder configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddPaymentGateway(
        this IServiceCollection services,
        [NotNull] IConfiguration config)
        => services
            .RegisterConfig<PaymentGatewayConfig, PaymentGatewayConfigValidator>(config)
            // ClickPay registration
            .RegisterHttpClient<ClickPayPaymentProvider>()
            .RegisterConfig<ClickPayConfig, ClickPayConfigValidator>(config)
            // Moyasar registration
            .RegisterHttpClient<MoyasarPaymentProvider>()
            .RegisterConfig<MoyasarConfig, MoyasarConfigValidator>(config)
            // Payment provider registration
            .AddScoped<IPaymentProvider>(sp =>
            {
                var config = sp.GetRequiredService<PaymentGatewayConfig>();
                return config.Provider switch
                {
                    PaymentProvider.Moyasar => sp.GetRequiredService<MoyasarPaymentProvider>(),
                    PaymentProvider.ClickPay => sp.GetRequiredService<ClickPayPaymentProvider>(),
                    _ => throw new NotSupportedException($"Payment provider {config.Provider} is not supported.")
                };
            });
}
