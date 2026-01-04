using Peers.Core.Cqrs.Pipeline;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Listings.Domain.Translations;
using Peers.Modules.Lookup.Domain;
using Peers.Modules.Lookup.Domain.Translations;

namespace Peers.Modules.Lookup.Commands;

public static class CreateLookupType
{
    /// <summary>
    /// Creates a lookup type with specified lookup constraints and localized names.
    /// </summary>
    /// <param name="Key">The unique key identifying the catalog entry. Cannot be null or empty.</param>
    /// <param name="ConstraintMode">The constraint mode that determines how lookups are validated for this entry.</param>
    /// <param name="AllowVariant">true if variants are allowed for this catalog entry; otherwise, false.</param>
    /// <param name="Names">An array of names for the catalog entry, each represented as a LookupTypeTr.Dto. Cannot be null.</param>
    [Authorize(Roles = Roles.CatalogManager)]
    public sealed record Command(
        string Key,
        LookupConstraintMode ConstraintMode,
        bool AllowVariant,
        LookupTypeTr.Dto[] Names) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.Key).NotEmpty();
            RuleFor(p => p.ConstraintMode).IsInEnum();
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
                .AnyAsync(p => p.Key == cmd.Key, ctk))
            {
                return Result.Conflict("Lookup type with the same key already exists.");
            }

            var lookupType = new LookupType(cmd.Key, cmd.ConstraintMode, cmd.AllowVariant);
            lookupType.UpsertTranslations(cmd.Names);
            _context.LookupTypes.Add(lookupType);
            await _context.SaveChangesAsync(ctk);

            return Result.Created(value: new IdObj(lookupType.Id));
        }
    }
}
