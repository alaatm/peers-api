using System.Text.Json.Serialization;
using Peers.Core.Cqrs.Pipeline;
using Peers.Modules.Catalog.Domain.Attributes;

namespace Peers.Modules.Catalog.Commands;

public static class Clone
{
    /// <summary>
    /// Clones an existing product type, creating a new version of it.
    /// </summary>
    /// <param name="Id">The unique identifier of the product type to clone.</param>
    /// <param name="CopyAttributes">Whether to copy attributes from the original product type to the new version.</param>
    [Authorize(Roles = Roles.CatalogManager)]
    public sealed record Command(
        [property: JsonIgnore()] int Id,
        bool CopyAttributes) : ICommand, IValidatable;

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
                .Include(p => p.Parent)
                .Include(p => p.Attributes).ThenInclude(p => p.Translations)
                .Include(p => p.Attributes).ThenInclude(p => (p as EnumAttributeDefinition)!.Options).ThenInclude(p => p.Translations)
                .Include(p => p.Translations)
                .FirstOrDefaultAsync(p => p.Id == cmd.Id, ctk) is not { } pt)
            {
                return Result.NotFound();
            }

            var clone = pt.CloneAsNextVersion(cmd.CopyAttributes);
            await _context.SaveChangesAsync(ctk);

            return Result.Created(value: new IdObj(clone.Id));
        }
    }
}
