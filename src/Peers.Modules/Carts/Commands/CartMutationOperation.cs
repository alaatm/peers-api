using System.Globalization;
using System.Text;
using Peers.Modules.Carts.Domain;
using Peers.Modules.Carts.Queries;
using Peers.Modules.Listings.Domain;
using Peers.Modules.Ordering.Domain;

namespace Peers.Modules.Carts.Commands;

internal static class CartMutationOperation
{
    private static readonly CompositeFormat _cartOperLockName = CompositeFormat.Parse("CARTMOD:{0}-{1}");

    public static async Task<IResult> ExecuteAsync(
        PeersContext context,
        TimeProvider timeProvider,
        IIdentityInfo identity,
        ILogger log,
        int listingId,
        string variantKey,
        Action<Cart, Listing> mutate,
        CancellationToken ctk)
    {
        var buyer = await context.Customers
            .FirstAsync(c => c.Id == identity.Id, ctk);

        // Do not filter on published listings here as Remove/Update may refer to a listing that has since been unpublished.
        // Cart.AddLineItem will ensure the listing is published before adding it to the cart.
        if (await context.Listings
            .AsNoTracking()
            .Include(p => p.Seller)
            .Include(p => p.Variants.Where(p => p.VariantKey == variantKey))
            .FirstOrDefaultAsync(p => p.Id == listingId, ctk) is not { } listing)
        {
            return Result.NotFound();
        }

        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await context.Database.BeginTransactionAsync(ctk);

            var resource = string.Format(CultureInfo.InvariantCulture, _cartOperLockName, identity.Id, listing.SellerId);
            if ((await context.AcquireAppLockAsync(transaction, resource, 10_000, ctk)) is < 0 and var rc)
            {
                return Result.Conflict("Another operation is updating this cart. Please retry shortly.");
            }

            if (await context.Carts
                .Include(p => p.Lines)
                .SingleOrDefaultAsync(p =>
                    p.BuyerId == identity.Id &&
                    p.SellerId == listing.SellerId, ctk) is not Cart cart)
            {
                cart = Cart.Create(buyer, listing.Seller, timeProvider.UtcNow());
                context.Carts.Add(cart);
            }

            if (await context.Orders
                .Include(o => o.Lines).ThenInclude(p => p.Variant)
                .SingleOrDefaultAsync(p =>
                    p.BuyerId == identity.Id &&
                    p.SellerId == listing.SellerId &&
                    p.State == OrderState.Placed, ctk) is { } placed)
            {
                cart.Restore(placed, timeProvider.UtcNow());
                placed.Cancel(OrderCancellationReason.Amended);
            }

            mutate(cart, listing);

            try
            {
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                var lines = new GetCart.Response.CartLineDto[cart.Lines.Count];
                for (var i = 0; i < cart.Lines.Count; i++)
                {
                    var line = cart.Lines[i];
                    lines[i] = new GetCart.Response.CartLineDto(
                        line.ListingId,
                        line.Variant.VariantKey,
                        line.Listing.Title,
                        line.Quantity,
                        line.UnitPrice);
                }

                var response = new GetCart.Response(lines);
                return Result.Ok(response);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                log.CartMutationOperationConcurrency(ex, identity.Id, listing.SellerId);
                return Result.Conflict("Another operation is updating this cart. Please retry shortly.");
            }
        });
    }
}
