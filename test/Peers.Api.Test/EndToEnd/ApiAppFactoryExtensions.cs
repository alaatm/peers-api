using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Peers.Core.Identity;
using Peers.Core.Nafath.Models;
using Peers.Core.Payments.Providers.ClickPay.Configuration;
using Peers.Core.Security.Jwt;
using Peers.Modules.Carts.Domain;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Kernel;
using Peers.Modules.Listings.Commands;
using Peers.Modules.Listings.Domain;
using Peers.Modules.Listings.Domain.Logistics;
using Peers.Modules.Sellers.Domain;
using Peers.Modules.Users.Domain;

namespace Peers.Api.Test.EndToEnd;

public static class ApiAppFactoryExtensions
{
    public static Customer CreateCustomer(this ApiAppFactory factory, string username, string phoneNumber, PeersContext existingContext = null)
    {
        using var scope = factory.Services.CreateScope();
        var context = existingContext is not null ? existingContext : scope.ServiceProvider.GetRequiredService<PeersContext>();

        var customer = Customer.Create(AppUser.CreateTwoFactorAccount(DateTime.UtcNow, username, phoneNumber, "en"), "secret");
        customer.AddAddress("Home", new("1", "1", "1", "1", "1", "1", "1", "1", new(0, 0)), true);
        context.Customers.Add(customer);
        context.SaveChanges();

        return customer;
    }

    public static Seller CreateSeller(this ApiAppFactory factory, string username, string phoneNumber, PeersContext existingContext = null)
    {
        using var scope = factory.Services.CreateScope();
        var context = existingContext is not null ? existingContext : scope.ServiceProvider.GetRequiredService<PeersContext>();

        var nafathIdentity = new NafathIdentity("1111111111", "احمد", "علي", "Ahmed", "Ali", default);
        var seller = Seller.Create(AppUser.CreateTwoFactorAccount(DateTime.UtcNow, username, phoneNumber, "en"), nafathIdentity, DateTime.UtcNow);
        context.Sellers.Add(seller);
        context.SaveChanges();
        return seller;
    }

    public static CheckoutSession CreateCheckoutSession(this ApiAppFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<PeersContext>();

        var buyer = CreateCustomer(factory, "buyer1", "+966511111111", context);
        var seller = CreateSeller(factory, "seller1", "+966522222222", context);

        var now = DateTime.UtcNow;

        var ptElectronics = ProductType.CreateRoot(ProductTypeKind.Physical, "Electronics");
        ptElectronics.DefineAttribute("brand", AttributeKind.String, true, false, 0);
        ptElectronics.Publish();
        var ptPhones = ptElectronics.AddChild("Phones", true, true);
        ptPhones.Publish();

        context.ProductTypes.Add(ptElectronics);
        context.SaveChanges();

        var fulfillment = new FulfillmentPreferences(FulfillmentMethod.PlatformManaged, ShippingCostPayer.Seller, null, true, null, null);
        var galaxyS10 = Listing.Create("Galaxy S10", seller, ptPhones, fulfillment, null, null, null, 100, now);

        context.Listings.Add(galaxyS10);
        context.SaveChanges();

        galaxyS10.SetAttributes(galaxyS10.Snapshot.SnapshotId, new Dictionary<string, SetAttributes.Command.AttributeInputDto>
        {
            ["brand"] = new SetAttributes.Command.AttributeInputDto.OptionCodeOrScalarString("samsung"),
        }, 10, 10, now);
        galaxyS10.SetLogistics(galaxyS10.Variants.First().SkuCode, new(new Dimensions(1, 1, 1), 1, false, false, TemperatureControl.None));
        galaxyS10.Publish(DateTime.UtcNow);

        context.SaveChanges();

        var cart = Cart.Create(buyer, seller, now);
        cart.AddLineItem(galaxyS10, galaxyS10.Variants.First().VariantKey, 1, now);
        cart.TryCheckout(buyer.AddressList.First(), null, 10, now, out var session, out _);
        session.MarkIntentIssued(now);

        context.Carts.Add(cart);
        context.SaveChanges();

        return session;
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

    public static CheckoutSession GetSession(this ApiAppFactory factory, int id)
    {
        using var scope = factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<PeersContext>();
        var session = context.CheckoutSessions
            .Include(s => s.Cart)
            .ThenInclude(c => c.Buyer)
            .First(s => s.Id == id);

        return session;
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
