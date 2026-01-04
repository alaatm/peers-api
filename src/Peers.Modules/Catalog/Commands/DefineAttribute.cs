using System.Text.Json.Serialization;
using Peers.Core.Cqrs.Pipeline;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Catalog.Domain.Translations;
using Peers.Modules.Lookup.Domain;

namespace Peers.Modules.Catalog.Commands;

public static class DefineAttribute
{
    /// <summary>
    /// Creates a new attribute definition for a product type.
    /// </summary>
    /// <param name="Id">The unique identifier of the product type to which the attribute will be added</param>
    /// <param name="Key">The unique key that identifies the attribute within the catalog.</param>
    /// <param name="Kind">The kind of attribute, specifying its data type.</param>
    /// <param name="IsRequired">A value indicating whether the attribute is required for catalog items.</param>
    /// <param name="IsVariant">A value indicating whether the attribute is used to define product variants.</param>
    /// <param name="Position">The display order of the attribute among other attributes. Lower values indicate higher priority in display.</param>
    /// <param name="LookupTypeKey">The key of the lookup type associated with the attribute, if applicable. Specify <see langword="null"/> if the attribute does not use a lookup.</param>
    /// <param name="Unit">The unit of measurement for the attribute, if applicable. Specify <see langword="null"/> if the attribute does not have a unit.</param>
    /// <param name="Min">The minimum allowed value for the attribute, if applicable. Specify <see langword="null"/> if there is no minimum constraint.</param>
    /// <param name="Max">The maximum allowed value for the attribute, if applicable. Specify <see langword="null"/> if there is no maximum constraint.</param>
    /// <param name="Step">The increment step for the attribute's value, if applicable. Specify <see langword="null"/> if there is no step constraint.</param>
    /// <param name="Regex">A regular expression pattern used to validate the attribute's value, if applicable. Specify <see langword="null"/> if no pattern validation is required.</param>
    /// <param name="Names">The localized names for the attribute.</param>
    [Authorize(Roles = Roles.CatalogManager)]
    public sealed record Command(
        [property: JsonIgnore()] int Id,
        string Key,
        AttributeKind Kind,
        bool IsRequired,
        bool IsVariant,
        int Position,
        string? LookupTypeKey,
        string? Unit,
        decimal? Min,
        decimal? Max,
        decimal? Step,
        string? Regex,
        AttributeDefinitionTr.Dto[] Names) : ICommand, IValidatable;

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
                .Include(p => p.Attributes)
                    .ThenInclude(p => ((LookupAttributeDefinition)p).LookupType.ParentLinks)
                .FirstOrDefaultAsync(p => p.Id == cmd.Id, ctk) is not { } pt)
            {
                return Result.NotFound();
            }

            LookupType? lookupType = null;

            if (!string.IsNullOrWhiteSpace(cmd.LookupTypeKey))
            {
                lookupType = await _context
                    .LookupTypes
                    .Include(p => p.ParentLinks)
                    .FirstOrDefaultAsync(lt => lt.Key == cmd.LookupTypeKey, ctk);

                if (lookupType is null)
                {
                    return Result.BadRequest(detail: "Lookup type not found.");
                }
            }

            var def = pt.DefineAttribute(cmd.Key, cmd.Kind, cmd.IsRequired, cmd.IsVariant, cmd.Position, lookupType, cmd.Unit, cmd.Min, cmd.Max, cmd.Step, cmd.Regex);
            def.UpsertTranslations(cmd.Names);
            await _context.SaveChangesAsync(ctk);

            return Result.Ok();
        }
    }
}
