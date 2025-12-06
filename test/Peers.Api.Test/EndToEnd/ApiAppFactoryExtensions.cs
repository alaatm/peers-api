using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Peers.Core.Identity;
using Peers.Core.Payments;
using Peers.Core.Payments.Providers.ClickPay.Configuration;
using Peers.Core.Security.Jwt;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Kernel;
using Peers.Modules.Users.Domain;

namespace Peers.Api.Test.EndToEnd;

public static class ApiAppFactoryExtensions
{
    public static Customer CreateCustomer(this ApiAppFactory factory, string username, string phoneNumber)
    {
        using var scope = factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<PeersContext>();
        var customer = Customer.Create(AppUser.CreateTwoFactorAccount(DateTime.UtcNow, username, phoneNumber, "en"), "secret");
        context.Customers.Add(customer);
        context.SaveChanges();

        return customer;
    }

    public static Customer CreateCustomerWithTokenizedCard(this ApiAppFactory factory, string username, string phoneNumber, string paymentId, string cardToken, bool hasMetadata = false)
    {
        using var scope = factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<PeersContext>();
        var customer = Customer.Create(AppUser.CreateTwoFactorAccount(DateTime.UtcNow, username, phoneNumber, "en"), "secret");

        PaymentCardBrand? brand = hasMetadata ? PaymentCardBrand.Visa : null;
        PaymentCardFunding? funding = hasMetadata ? PaymentCardFunding.Credit : null;
        DateOnly? expiry = hasMetadata ? new DateOnly(2025, 12, 31) : null;

        customer.AddPaymentCard(paymentId, brand, funding, "4666-66XX-XXXX-8888", expiry, cardToken, DateTime.UtcNow);
        context.Customers.Add(customer);
        context.SaveChanges();

        return customer;
    }

    public static Customer GetCustomer(this ApiAppFactory factory, int id)
    {
        using var scope = factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<PeersContext>();
        var customer = context.Customers
            .Include(c => c.PaymentMethods)
            .First(c => c.Id == id);

        return customer;
    }

    public static string GenerateAndCacheTokenId(this ApiAppFactory factory, Customer customer, out string cacheKey)
    {
        cacheKey = TokenIdResolver.GenerateTokenIdCacheKey(out var tokenId);
        var cache = factory.Services.GetRequiredService<IMemoryCache>();
        cache.Set(cacheKey, factory.CreateBearerToken(customer.Id, customer.Username));

        return tokenId;
    }

    public static string CreateBearerToken(this ApiAppFactory factory, int userId, string username)
    {
        var jwtProvider = factory.Services.GetRequiredService<IJwtProvider>();
        var (token, _) = jwtProvider.BuildToken([Roles.Customer],
        [
            new(CustomClaimTypes.Id, userId.ToString()),
            new(CustomClaimTypes.Username, username),
        ]);

        return token;
    }

    public static void SetClickPayConfig(this ApiAppFactory factory, string profileId, string key)
    {
        var config = factory.Services.GetRequiredService<ClickPayConfig>();
        config.ProfileId = profileId;
        config.Key = key;
    }
}
