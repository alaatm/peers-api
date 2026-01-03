using NetTopologySuite.Geometries;
using Peers.Core.Shipping;

namespace Peers.Modules.Carts.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCartServices(this IServiceCollection services) => services
        .AddScoped<IShippingCalculator, ShippingCalculator>()
        .AddScoped<IPlatformShippingService, PlatformShippingService>()
        .AddScoped<IPaymentProcessor, PaymentProcessor>();
}

public class PlatformShippingService : IPlatformShippingService
{
    public Task<decimal> ComputeAsync(Point deliveryLocation, PlatformShipmentItem[] items, CancellationToken ctk) => throw new NotImplementedException();
}
