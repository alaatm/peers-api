using Peers.Core.Cqrs.Pipeline;
using Peers.Core.Localization.Infrastructure;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Translations;

namespace Peers.Modules.Catalog.Commands;

public static class CreateRoot
{
    /// <summary>
    /// Creates a root product type.
    /// </summary>
    /// <param name="Kind">The kind of product type.</param>
    /// <param name="Names">The localized names.</param>
    [Authorize(Roles = Roles.CatalogManager)]
    public sealed record Command(
        ProductTypeKind Kind,
        ProductTypeTr.Dto[] Names) : ICommand, IValidatable;


    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.Kind).IsInEnum();
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
            var enName = cmd.Names.GetEn()!.Name;
            var slugPath = $"/{SlugHelper.ToSlug(enName)}";
            if (_context.ProductTypes.Any(pt => pt.SlugPath == slugPath))
            {
                return Result.Conflict(detail: "A product type with the same name already exists.");
            }

            var productType = ProductType.CreateRoot(cmd.Kind, enName);
            productType.UpsertTranslations(cmd.Names);

            _context.ProductTypes.Add(productType);
            await _context.SaveChangesAsync(ctk);

            return Result.Created(value: new IdObj(productType.Id));
        }
    }
}
