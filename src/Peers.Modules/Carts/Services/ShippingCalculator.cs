using NetTopologySuite.Geometries;
using Peers.Core.Domain.Errors;
using Peers.Core.Shipping;
using Peers.Modules.Carts.Domain;
using Peers.Modules.Listings.Domain.Logistics;
using Peers.Modules.Listings.Services;
using Peers.Modules.Sellers.Domain;

namespace Peers.Modules.Carts.Services;

/// <summary>
/// Represents a service that calculates shipping costs for a given cart and delivery location.
/// </summary>
public sealed class ShippingCalculator : IShippingCalculator
{
    private readonly IPlatformShippingService _platform;
    private readonly IDistanceCalculator _distance;
    private const int VolumetricDivisor = 5000; // TODO: Move to configuration

    public ShippingCalculator(
        IPlatformShippingService platform,
        IDistanceCalculator distance)
    {
        _platform = platform;
        _distance = distance;
    }

    /// <summary>
    /// Asynchronously calculates the shipping options and costs for the specified cart and delivery location.
    /// </summary>
    /// <param name="cart">The shopping cart containing the items for which to calculate shipping. Cannot be null.</param>
    /// <param name="deliveryLocation">The destination location where the order will be delivered. Cannot be null.</param>
    /// <param name="ctk">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a ShippingCalculatorResult with the
    /// available shipping options and their associated costs.</returns>
    public async Task<ShippingCalculatorResult> CalculateAsync(
        [NotNull] Cart cart,
        [NotNull] Point deliveryLocation,
        CancellationToken ctk = default)
    {
        if (cart.Lines is null || cart.Lines.Count == 0)
        {
            return new ShippingCalculatorResult(ShippingCalculationOutcome.Success, 0m);
        }

        // It is guaranteed that all line items in the cart have one of the following fulfillment methods:
        // 1. Platform-managed shipping.
        // 2. Seller-managed non-quote-based shipping.
        // 3. Seller-managed quote-based shipping.

        // Take method from the first line's listing
        var line0 = cart.Lines[0];
        var method = line0.Listing.FulfillmentPreferences.Method;

        if (!cart.Lines.All(p => p.Listing.FulfillmentPreferences.Method == method))
        {
            throw new InvalidDomainStateException("All cart lines must have the same fulfillment method.");
        }

        if (line0.Listing.IsSellerManagedQuoteBasedShipping)
        {
            return ShippingCalculatorResult.QuoteRequired();
        }

        return method switch
        {
            FulfillmentMethod.PlatformManaged => await CalculatePlatformAsync(cart, deliveryLocation, ctk),
            FulfillmentMethod.SellerManaged => await CalculateSellerManagedAsync(cart, deliveryLocation, ctk),
            _ => throw new InvalidOperationException("Unsupported fulfillment method for shipping calculation."),
        };
    }

    private async Task<ShippingCalculatorResult> CalculatePlatformAsync(
        Cart cart,
        Point deliveryLocation,
        CancellationToken ctk)
    {
        // Map cart lines to platform items
        var items = cart.Lines.Select(line =>
        {
            var logistics = line.Variant.Logistics
                ?? throw new InvalidDomainStateException("Variant logistics must be set for platform-managed shipping.");

            return new PlatformShipmentItem(
                Weight: logistics.Weight * line.Quantity,
                Length: logistics.Dimensions.Length,
                Width: logistics.Dimensions.Width,
                Height: logistics.Dimensions.Height,
                Quantity: line.Quantity);
        }).ToArray();

        var amount = await _platform.ComputeAsync(deliveryLocation, items, ctk);

        return new ShippingCalculatorResult(
            ShippingCalculationOutcome.Success,
            amount);
    }

    private async Task<ShippingCalculatorResult> CalculateSellerManagedAsync(
        Cart cart,
        Point deliveryLocation,
        CancellationToken ctk)
    {
        // Group by seller shipping profile
        var groups = cart.Lines
            .GroupBy(p => p.Listing.ShippingProfile ?? throw new InvalidDomainStateException("Seller-managed listing must have a shipping profile."))
            .ToArray();

        var total = 0m;

        foreach (var group in groups)
        {
            var profile = group.Key;

            // subtotal for this profile group
            var groupSubtotal = group.Sum(l => l.LineTotal);

            // compute billable weight for the group
            var groupWeightKg = group
                .Sum(p =>
                {
                    var logistics = p.Variant.Logistics ?? throw new InvalidDomainStateException("Variant logistics must be set for seller-managed shipping.");
                    return logistics.BillableWeight(VolumetricDivisor) * p.Quantity;
                });

            var distanceMeters =
                profile.Rate.Kind is SellerManagedRateKind.Distance ||
                profile.FreeShippingPolicy is not null
                    ? await _distance.MeasureAsync(
                        profile.OriginLocation ?? throw new InvalidOperationException("Shipping location expected but missing."),
                        deliveryLocation,
                        ctk)
                    : 0;

            var amount = profile.Rate.ComputeBuyerCharge(
                profile,
                groupSubtotal,
                groupWeightKg,
                distanceMeters);

            total += amount;
        }

        return new ShippingCalculatorResult(
            ShippingCalculationOutcome.Success,
            total);
    }
}
