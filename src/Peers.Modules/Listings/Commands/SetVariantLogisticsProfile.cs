using System.Text.Json.Serialization;
using Peers.Core.Cqrs.Pipeline;
using Peers.Modules.Listings.Domain.Logistics;

namespace Peers.Modules.Listings.Commands;

public static class SetVariantLogisticsProfile
{
    /// <summary>
    /// Sets the logistics profile for a specific variant, or all variants within a product listing.
    /// </summary>
    /// <param name="Id">The identifier of the product listing that the variant belongs to.</param>
    /// <param name="Sku">The SKU of the listing variant to set the logistics profile for. If <c>null</c>, the logistics profile will be set for all variants.</param>
    /// <param name="Profile">The logistics profile to set for the listing variant.</param>
    [Authorize(Roles = Roles.Seller)]
    public sealed record Command(
        [property: JsonIgnore()] int Id,
        [property: JsonIgnore()] string? Sku,
        LogisticsProfile Profile) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.Id).GreaterThan(0);
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;

        public Handler(PeersContext context)
            => _context = context;

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            var q = _context.Listings
                .Include(p => p.ProductType)
                .AsQueryable();

            if (cmd.Sku is not null)
            {
                q = q.Include(p => p.Variants.Where(p => p.SkuCode == cmd.Sku));
            }
            else
            {
                q = q.Include(p => p.Variants);
            }

            if (await q.FirstOrDefaultAsync(p => p.Id == cmd.Id, ctk) is not { } listing)
            {
                return Result.BadRequest(detail: "Listing not found.");
            }

            listing.SetLogistics(cmd.Sku, cmd.Profile);
            await _context.SaveChangesAsync(ctk);
            return Result.Ok();
        }
    }
}
