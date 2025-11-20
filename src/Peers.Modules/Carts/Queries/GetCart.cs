using NetTopologySuite.Geometries;
using Peers.Core.Geo;
using Peers.Modules.Carts.Domain;
using Peers.Modules.Carts.Services;

namespace Peers.Modules.Carts.Queries;

public static class GetCart
{
    private static readonly IResult _emptyResult = Result.Ok(new Response(0, false, []));
    private static readonly Point _defaultLocation = GeometryHelper.CreatePoint(24.752077613768105, 46.672593596226065);

    /// <summary>
    /// Retrieve the shopping cart for the current buyer and specified seller.
    /// </summary>
    /// <param name="SellerId">The unique identifier of the seller.</param>
    /// <param name="UserLocation">The geographical location of the user, used for shipping calculations only when the customer has no default address.</param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Query(
        int SellerId,
        Point? UserLocation) : IQuery;

    /// <summary>
    /// Represents the result of a cart operation, including shipping fee information and the items in the
    /// cart.
    /// </summary>
    /// <param name="ShippingFee">The total shipping fee for the order, or null if the shipping fee is not yet determined.</param>
    /// <param name="RequiresShippingQuote">true if a shipping quote is required before completing the order; otherwise, false.</param>
    /// <param name="Lines">An array of cart line items included in the response. Each item contains details about a product in the cart.</param>
    public sealed record Response(
        decimal? ShippingFee,
        bool RequiresShippingQuote,
        Response.CartLineDto[] Lines)
    {
        /// <summary>
        /// Represents a single item in a shopping cart, including product details, quantity, and pricing information.
        /// </summary>
        /// <param name="ListingId">The unique identifier of the product listing associated with this cart line.</param>
        /// <param name="VariantKey">A key that identifies the specific variant of the product, such as size or color. Cannot be null.</param>
        /// <param name="Title">The display title or name of the product. Cannot be null.</param>
        /// <param name="Quantity">The number of units of the product in the cart. Must be greater than zero.</param>
        /// <param name="UnitPrice">The price per single unit of the product, before any discounts or taxes.</param>
        public sealed record CartLineDto(
            int ListingId,
            string VariantKey,
            string Title,
            int Quantity,
            decimal UnitPrice);
    }

    public sealed class Handler : ICommandHandler<Query>
    {
        private readonly PeersContext _context;
        private readonly IShippingCalculator _shippingCalculator;
        private readonly IIdentityInfo _identity;

        public Handler(
            PeersContext context,
            IShippingCalculator shippingCalculator,
            IIdentityInfo identity)
        {
            _context = context;
            _shippingCalculator = shippingCalculator;
            _identity = identity;
        }

        public async Task<IResult> Handle([NotNull] Query cmd, CancellationToken ctk)
        {
            if (await _context.Carts
                .Include(p => p.Buyer).ThenInclude(p => p.AddressList.Where(p => p.IsDefault))
                .Include(p => p.Lines)
                .SingleOrDefaultAsync(p =>
                    p.BuyerId == _identity.Id &&
                    p.SellerId == cmd.SellerId, ctk) is not Cart cart)
            {
                return _emptyResult;
            }

            var lines = new Response.CartLineDto[cart.Lines.Count];
            for (var i = 0; i < cart.Lines.Count; i++)
            {
                var line = cart.Lines[i];
                lines[i] = new Response.CartLineDto(
                    line.ListingId,
                    line.Variant.VariantKey,
                    line.Listing.Title,
                    line.Quantity,
                    line.UnitPrice);
            }

            var deliveryAddress = cart.Buyer.GetDefaultAddress()?.Location ?? cmd.UserLocation ?? _defaultLocation;

            var shippingCalcResult = await _shippingCalculator.CalculateAsync(cart, deliveryAddress, ctk);
            var requiresQuote = shippingCalcResult.Outcome is ShippingCalculationOutcome.QuoteRequired;
            decimal? shippingFee = shippingCalcResult.Outcome is ShippingCalculationOutcome.Success
                ? shippingCalcResult.Total
                : null;

            return Result.Ok(new Response(
                shippingFee,
                requiresQuote,
                lines));
        }
    }
}
