using System.Text.Json.Serialization;
using Peers.Core.Cqrs.Pipeline;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Catalog.Domain.Translations;

namespace Peers.Modules.Catalog.Commands;

public static class AddEnumAttributeOption
{
    /// <summary>
    /// Adds a new option to an enum attribute definition of a product type.
    /// </summary>
    /// <param name="Id">The unique identifier of the product type to which the attribute belongs.</param>
    /// <param name="Key">The unique key that identifies the enum attribute within the catalog that the option should be added to.</param>
    /// <param name="OptionCode">The unique code representing the attribute option.</param>
    /// <param name="ParentOptionCode">The code of the parent option if this option is a child; otherwise, <see langword="null"/>.</param>
    /// <param name="Position">The display order of the attribute option among other options. Lower values indicate higher priority in display.</param>
    /// <param name="Names">The localized names for the attribute option.</param>
    [Authorize(Roles = Roles.CatalogManager)]
    public sealed record Command(
        [property: JsonIgnore()] int Id,
        [property: JsonIgnore()] string Key,
        string OptionCode,
        int Position,
        string? ParentOptionCode,
        EnumAttributeOptionTr.Dto[] Names) : ICommand, IValidatable;

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
                .Include(p => p.Attributes.Where(p => p.Key == cmd.Key))
                    .ThenInclude(p => ((EnumAttributeDefinition)p).Options)
                .FirstOrDefaultAsync(p => p.Id == cmd.Id, ctk) is not { } pt)
            {
                return Result.NotFound();
            }

            var opt = pt.AddAttributeOption(cmd.Key, cmd.OptionCode, cmd.Position, cmd.ParentOptionCode);
            opt.UpsertTranslations(cmd.Names);
            await _context.SaveChangesAsync(ctk);

            return Result.Ok();
        }
    }
}
