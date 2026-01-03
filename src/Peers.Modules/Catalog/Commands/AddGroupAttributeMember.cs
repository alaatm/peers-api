using System.Text.Json.Serialization;
using Peers.Core.Cqrs.Pipeline;
using Peers.Modules.Catalog.Domain;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Catalog.Domain.Translations;

namespace Peers.Modules.Catalog.Commands;

public static class AddGroupAttributeMember
{
    /// <summary>
    /// Adds a group member to a group attribute definition of a product type.
    /// </summary>
    /// <param name="Id">The unique identifier of the product type to which the attribute belongs.</param>
    /// <param name="Key">The unique key that identifies the group attribute within the catalog that the member should be added to.</param>
    /// <param name="MemberKey">The unique key representing the group attribute member.</param>
    [Authorize(Roles = Roles.CatalogManager)]
    public sealed record Command(
        [property: JsonIgnore()] int Id,
        [property: JsonIgnore()] string Key,
        string MemberKey) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.Id).GreaterThan(0);
            RuleFor(p => p.Key).NotEmpty();
            RuleFor(p => p.MemberKey).NotEmpty();
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
                .Include(p => p.Attributes.Where(p => p.Key == cmd.Key || p.Key == cmd.MemberKey))
                    .ThenInclude<ProductType, AttributeDefinition, List<NumericAttributeDefinition>>(p => ((GroupAttributeDefinition)p).Members)
                .FirstOrDefaultAsync(p => p.Id == cmd.Id, ctk) is not { } pt)
            {
                return Result.NotFound();
            }

            pt.AddGroupAttributeMember(cmd.Key, cmd.MemberKey);
            await _context.SaveChangesAsync(ctk);

            return Result.Ok();
        }
    }
}
