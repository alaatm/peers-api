using System.Diagnostics;
using System.Runtime.CompilerServices;
using NetTopologySuite.Geometries;
using Peers.Core.Domain.Errors;
using Peers.Modules.Listings.Services;
using E = Peers.Modules.Listings.ListingErrors;

namespace Peers.Modules.Listings.Domain.Logistics;

/// <summary>
/// Pricing rules for seller-managed shipping: flat, weight-based, distance-based, or quote-based.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Kind"/> determines which fields are used:
/// <list type="bullet">
///   <item><description><b>Flat</b>: Uses <see cref="FlatAmount"/> only.</description></item>
///   <item><description><b>Weight</b>: Uses <see cref="RatePerKg"/>, optional <see cref="BaseFee"/>, and optional <see cref="MinFee"/>.</description></item>
///   <item><description><b>Distance</b>: Uses <see cref="RatePerKm"/>, optional <see cref="BaseFee"/>, and optional <see cref="MinFee"/>.</description></item>
///   <item><description><b>Quote</b>: Price cannot be computed at checkout; shipping price must be provided separately.</description></item>
/// </list>
/// Distances are in <b>kilometers</b>; weights are in <b>kilograms</b>.
/// </para>
/// </remarks>
public sealed class SellerManagedRate
{
    /// <summary>
    /// The calculation method for seller-managed shipments.
    /// </summary>
    public SellerManagedRateKind Kind { get; private set; }
    /// <summary>
    /// The flat shipping amount charged to the buyer (flat rate only).
    /// </summary>
    public decimal? FlatAmount { get; private set; }
    /// <summary>
    /// The base fee added to weight/distance-based charges.
    /// </summary>
    public decimal? BaseFee { get; private set; }
    /// <summary>
    /// The rate per kilogram charged to the buyer (weight-based).
    /// </summary>
    public decimal? RatePerKg { get; private set; }
    /// <summary>
    /// The rate per kilometer charged to the buyer (distance-based).
    /// </summary>
    public decimal? RatePerKm { get; private set; }
    /// <summary>
    /// The minimum total shipping fee (weight/distance-based).
    /// </summary>
    public decimal? MinFee { get; private set; }

    private SellerManagedRate() { }

    /// <summary>
    /// Creates a fixed flat-rate configuration.
    /// </summary>
    /// <param name="amount">The flat amount to charge.</param>
    public static SellerManagedRate Flat(decimal amount) => new()
    {
        Kind = SellerManagedRateKind.Flat,
        FlatAmount = amount
    };

    /// <summary>
    /// Creates a weight-based configuration.
    /// </summary>
    /// <remarks>
    /// Used to define shipping rates where the cost is determined by multiplying the
    /// shipment's weight by a per-kilogram rate, optionally including a base fee and enforcing a minimum charge.
    /// </remarks>
    /// <param name="ratePerKg">The rate per kilogram.</param>
    /// <param name="baseFee">An optional base fee. Defaults to 0 if not specified.</param>
    /// <param name="minFee">An optional minimum total fee (applied after calculation).</param>
    public static SellerManagedRate Weight(
        decimal ratePerKg,
        decimal baseFee = 0,
        decimal? minFee = null) => new()
        {
            Kind = SellerManagedRateKind.Weight,
            RatePerKg = ratePerKg,
            BaseFee = baseFee,
            MinFee = minFee
        };

    /// <summary>
    /// Creates a distance-based configuration.
    /// </summary>
    /// <param name="ratePerKm">The rate per kilometer.</param>
    /// <param name="baseFee">An optional base fee. Defaults to 0 if not specified.</param>
    /// <param name="minFee">An optional minimum total fee (applied after calculation).</param>
    public static SellerManagedRate Distance(
        decimal ratePerKm,
        decimal baseFee = 0,
        decimal? minFee = null) => new()
        {
            Kind = SellerManagedRateKind.Distance,
            RatePerKm = ratePerKm,
            BaseFee = baseFee,
            MinFee = minFee
        };

    /// <summary>
    /// Creates a configuration indicating the price must be quoted manually.
    /// </summary>
    public static SellerManagedRate Quote() => new()
    {
        Kind = SellerManagedRateKind.Quote
    };

    /// <summary>
    /// Computes buyer-visible shipping charge for seller-managed listings.
    /// Applies payer/free rules before pricing. For quote-based rates, a price cannot be computed at checkout.
    /// Distances are in kilometers; weights in kilograms.
    /// </summary>
    /// <param name="distanceCalculator">The service used to measure origin → delivery distance (returns meters).</param>
    /// <param name="prefs">The Fulfillment preferences.</param>
    /// <param name="deliveryLocation">The delivery destination.</param>
    /// <param name="orderSubtotal">The order subtotal in store currency.</param>
    /// <param name="totalWeightKg">The total shipment weight in kilograms. Must be non-negative.</param>
    /// <param name="ctk">Optional cancellation token.</param>
    public async Task<decimal> ComputeBuyerChargeAsync(
        [NotNull] IDistanceCalculator distanceCalculator,
        [NotNull] FulfillmentPreferences prefs,
        [NotNull] Point deliveryLocation,
        decimal orderSubtotal,
        double totalWeightKg,
        CancellationToken ctk = default)
    {
        Debug.Assert(prefs.OutboundPaidBy is ShippingCostPayer.Buyer);
        ArgumentOutOfRangeException.ThrowIfNegative(orderSubtotal);
        ArgumentOutOfRangeException.ThrowIfNegative(totalWeightKg);

        // Quote path: cannot compute at checkout; free-shipping policy does not apply.
        if (Kind is SellerManagedRateKind.Quote)
        {
            throw new DomainException(E.Logistics.CannotComputeQuoteRate);
        }

        var needsDistance =
            Kind is SellerManagedRateKind.Distance ||
            prefs.FreeShippingPolicy is not null;

        double? distanceMeters = needsDistance
            ? await distanceCalculator.MeasureAsync(
                prefs.OriginLocation ?? throw new InvalidOperationException("Shipping location expected but missing."),
                deliveryLocation,
                ctk)
            : null;

        if (prefs.FreeShippingPolicy is { } fsp &&
            fsp.IsSatisfiedBy(orderSubtotal, Ensure(distanceMeters)))
        {
            return 0m;
        }

        var result = Kind switch
        {
            SellerManagedRateKind.Flat => Ensure(FlatAmount),
            SellerManagedRateKind.Weight => ApplyMin(Ensure(BaseFee) + ((decimal)totalWeightKg * Ensure(RatePerKg))),
            SellerManagedRateKind.Distance => ApplyMin(Ensure(BaseFee) + ((decimal)Ensure(distanceMeters) / 1000m * Ensure(RatePerKm))),
            _ => throw new UnreachableException("Invalid seller-managed rate kind."),
        };

        return Math.Round(result, 2);

        decimal ApplyMin(decimal x) => MinFee.HasValue ? Math.Max(x, MinFee.Value) : x;
    }

    /// <summary>
    /// Validates the configuration. Throws if any required values are missing or invalid.
    /// </summary>
    internal void Validate()
    {
        switch (Kind)
        {
            case SellerManagedRateKind.Flat:
                if (FlatAmount is null or < 0)
                {
                    throw new DomainException(E.Logistics.InvalidFlatRate);
                }
                break;
            case SellerManagedRateKind.Weight:
                if (RatePerKg is null || RatePerKg < 0 || BaseFee is null || BaseFee < 0)
                {
                    throw new DomainException(E.Logistics.InvalidWeightRate);
                }
                if (MinFee.HasValue && MinFee < 0)
                {
                    throw new DomainException(E.Logistics.InvalidMinFee);
                }
                break;
            case SellerManagedRateKind.Distance:
                if (RatePerKm is null || RatePerKm < 0 || BaseFee is null || BaseFee < 0)
                {
                    throw new DomainException(E.Logistics.InvalidDistanceRate);
                }
                if (MinFee.HasValue && MinFee < 0)
                {
                    throw new DomainException(E.Logistics.InvalidMinFee);
                }
                break;
            case SellerManagedRateKind.Quote:
                if (FlatAmount is not null || RatePerKg is not null || RatePerKm is not null || BaseFee is not null || MinFee is not null)
                {
                    throw new DomainException(E.Logistics.QuoteRateMustNotSpecifyPricingFields);
                }
                break;
            default:
                throw new DomainException(E.Logistics.InvalidSellerRateKind);
        }
    }

    private static T Ensure<T>(T? value, [CallerArgumentExpression(nameof(value))] string? name = null)
        where T : struct =>
        value ?? throw new UnreachableException($"Expected '{name}' to be set before use.");
}
