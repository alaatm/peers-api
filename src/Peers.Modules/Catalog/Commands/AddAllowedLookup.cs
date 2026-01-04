using System.Text.Json.Serialization;
using Peers.Core.Cqrs.Pipeline;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Catalog.Domain.Translations;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Catalog.Commands;

public static class AddAllowedLookup
{
    /// <summary>
    /// Adds a lookup option to the list of allowed lookups for a lookup attribute definition.
    /// </summary>
    /// <param name="Id">The unique identifier of the product type to which the attribute belongs.</param>
    /// <param name="Key">The key that identifies the lookup attribute within the catalog.</param>
    /// <param name="Code">The code that identifies the lookup option to be added.</param>
    [Authorize(Roles = Roles.CatalogManager)]
    public sealed record Command(
        [property: JsonIgnore()] int Id,
        [property: JsonIgnore()] string Key,
        string Code) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.Id).GreaterThan(0);
            RuleFor(p => p.Key).NotEmpty();
            RuleFor(p => p.Code).NotEmpty();
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;

        public Handler(PeersContext context) => _context = context;

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            if (await _context
                .ProductTypes
                .Include(p => p.Attributes.Where(p => p.Key == cmd.Key))
                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).AllowedOptions).ThenInclude(p => p.Option)
                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType.ParentLinks)
                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType.ChildLinks)
                .FirstOrDefaultAsync(p => p.Id == cmd.Id, ctk) is not { } pt)
            {
                return Result.NotFound();
            }

            if (await _context
                .Set<LookupOption>()
                .Include(p => p.Type)
                .FirstOrDefaultAsync(p => p.Code == cmd.Code, ctk) is not { } lookupOption)
            {
                return Result.BadRequest(detail: "Lookup option not found.");
            }

            pt.AddAllowedLookup(cmd.Key, lookupOption);
            await _context.SaveChangesAsync(ctk);

            return Result.Ok();
        }
    }
}
