using System.Text.Json.Serialization;
using Peers.Core.Cqrs.Pipeline;
using Peers.Modules.Catalog.Domain.Attributes;

namespace Peers.Modules.Listings.Commands;

public static class Publish
{
    /// <summary>
    /// Publishes a product listing and make it available to buyers.
    /// </summary>
    /// <param name="Id">The identifier of the product listing to be published.</param>
    [Authorize(Roles = Roles.Seller)]
    public sealed record Command([property: JsonIgnore()] int Id) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator([NotNull] IStrLoc l)
            => RuleFor(p => p.Id).GreaterThan(0);
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;
        private readonly TimeProvider _timeProvider;

        public Handler(
            PeersContext context,
            TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            if (await _context.Listings
                .AsSplitQuery()
                .Include(p => p.Attributes)
                .Include(p => p.Variants)
                .Include(p => p.ProductType).ThenInclude(p => p.Index)
                .Include(p => p.ProductType).ThenInclude(p => p.Attributes).ThenInclude(p => ((EnumAttributeDefinition)p).Options)
                .Include(p => p.ProductType).ThenInclude(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).AllowedOptions)
                .Include(p => p.ProductType).ThenInclude(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(p => p.Options)
                .Include(p => p.ProductType).ThenInclude(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(p => p.ParentLinks)
                .Include(p => p.ProductType).ThenInclude(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(p => p.ChildLinks)
                .FirstOrDefaultAsync(p => p.Id == cmd.Id, ctk) is not { } listing)
            {
                return Result.BadRequest(detail: "Invalid listing.");
            }

            listing.Publish(_timeProvider.UtcNow());
            await _context.SaveChangesAsync(ctk);
            return Result.Ok();
        }
    }
}
