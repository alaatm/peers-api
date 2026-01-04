using System.Text.Json.Serialization;
using Peers.Core.Cqrs.Pipeline;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Listings.Domain.Translations;
using Peers.Modules.Lookup.Domain;
using Peers.Modules.Lookup.Domain.Translations;

namespace Peers.Modules.Lookup.Commands;

public static class AddLookupOption
{
    /// <summary>
    /// Adds a new lookup option entry to a specified lookup type.
    /// </summary>
    /// <param name="LookupTypeKey">The unique key of the lookup type to which this entry belongs. Must correspond to a valid catalog lookup type.</param>
    /// <param name="Code">The unique code for the lookup option. Cannot be null or empty. Used to identify the entry within its lookup
    /// type.</param>
    /// <param name="Names">An array of localized names for the option entry.</param>
    [Authorize(Roles = Roles.CatalogManager)]
    public sealed record Command(
        [property: JsonIgnore()] string LookupTypeKey,
        string Code,
        LookupOptionTr.Dto[] Names) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.LookupTypeKey).NotEmpty();
            RuleFor(p => p.Code).NotEmpty();
            RuleFor(p => p.Names).NotEmpty();
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;

        public Handler(
            PeersContext context)
        {
            _context = context;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            if (await _context
                .LookupTypes
                .FirstAsync(p => p.Key == cmd.LookupTypeKey, ctk) is not { } lookupType)
            {
                return Result.BadRequest(detail: "Lookup type not found.");
            }

            if (await _context
                .Set<LookupOption>()
                .AnyAsync(p => p.TypeId == lookupType.Id && p.Code == cmd.Code, ctk))
            {
                return Result.Conflict(detail: "Lookup option already exists for lookup type.");
            }

            var opt = lookupType.CreateOption(cmd.Code);
            opt.UpsertTranslations(cmd.Names);
            await _context.SaveChangesAsync(ctk);

            return Result.Created(value: new IdObj(opt.Id));
        }
    }
}
