using System.Text.Json.Serialization;
using Peers.Core.Cqrs.Pipeline;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Catalog.Domain.Translations;

namespace Peers.Modules.Catalog.Commands;

public static class AddChild
{
    /// <summary>
    /// Adds a child product type under a specified parent product type.
    /// </summary>
    /// <param name="ParentId">The parent product type ID.</param>
    /// <param name="IsSelectable">Whether the product type is selectable.</param>
    /// <param name="CopyAttributes">Whether to copy attributes from the parent.</param>
    /// <param name="Names">The localized names.</param>
    [Authorize(Roles = Roles.CatalogManager)]
    public sealed record Command(
        [property: JsonIgnore()] int ParentId,
        bool IsSelectable,
        bool CopyAttributes,
        ProductTypeTr.Dto[] Names) : ICommand, IValidatable;


    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.ParentId).GreaterThan(0);
            RuleFor(p => p.Names).NotNull().NotEmpty().Must(p => p.FirstOrDefault(p => p.LangCode == "en") is not null)
                .WithMessage(l["At least one name in English (en) is required."]);
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
                .Include(p => p.Children)
                .Include(p => p.Attributes).ThenInclude(p => p.Translations)
                .Include(p => p.Attributes).ThenInclude(p => (p as EnumAttributeDefinition)!.Options).ThenInclude(p => p.Translations)
                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).AllowedOptions)
                .Include(p => p.Attributes).ThenInclude(p => ((LookupAttributeDefinition)p).LookupType.ParentLinks)
                .Include(p => p.Translations)
                .FirstOrDefaultAsync(p => p.Id == cmd.ParentId, ctk) is not { } parent)
            {
                return Result.NotFound();
            }

            var child = parent.AddChild(cmd.Names.GetEn()!.Name, cmd.IsSelectable, cmd.CopyAttributes);
            child.UpsertTranslations(cmd.Names);
            await _context.SaveChangesAsync(ctk);

            return Result.Created(value: new IdObj(child.Id));
        }
    }
}
