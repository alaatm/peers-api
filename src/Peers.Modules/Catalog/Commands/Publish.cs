using System.Text.Json.Serialization;
using Peers.Core.Cqrs.Pipeline;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Catalog.Domain.Translations;

namespace Peers.Modules.Catalog.Commands;

public static class Publish
{
    /// <summary>
    /// Publishes the product type with the specified ID.
    /// </summary>
    /// <param name="Id">The product type ID.</param>
    [Authorize(Roles = Roles.CatalogManager)]
    public sealed record Command([property: JsonIgnore()] int Id) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator([NotNull] IStrLoc l)
            => RuleFor(p => p.Id).GreaterThan(0);
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;

        public Handler(PeersContext context) => _context = context;

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            if (await _context
                .ProductTypes
                .AsSplitQuery()
                .Include(p => p.Attributes).ThenInclude(p => ((EnumAttributeDefinition)p).Options)
                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).AllowedOptions)
                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(p => p.Options)
                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(p => p.ParentLinks).ThenInclude(p => p.ChildType)
                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType).ThenInclude(p => p.ChildLinks).ThenInclude(p => p.ParentType)
                .FirstOrDefaultAsync(p => p.Id == cmd.Id, ctk) is not { } pt)
            {
                return Result.NotFound();
            }

            pt.Publish();
            await _context.SaveChangesAsync(ctk);
            return Result.Ok();
        }
    }
}
