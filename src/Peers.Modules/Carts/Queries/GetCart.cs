using Peers.Modules.Carts.Domain;

namespace Peers.Modules.Carts.Queries;

public static class GetCart
{
    private static readonly IResult _emptyResult = Result.Ok(new Response([]));

    /// <summary>
    /// Retrieve the shopping cart for the current buyer and specified seller.
    /// </summary>
    /// <param name="SellerId">The unique identifier of the seller.</param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Query(int SellerId) : IQuery;

    public sealed record Response(Response.CartLineDto[] Lines)
    {
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
        private readonly IIdentityInfo _identity;

        public Handler(
            PeersContext context,
            IIdentityInfo identity)
        {
            _context = context;
            _identity = identity;
        }

        public async Task<IResult> Handle([NotNull] Query cmd, CancellationToken ctk)
        {
            if (await _context.Carts
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

            return Result.Ok(new Response(lines));
        }
    }
}
