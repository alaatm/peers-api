using System.Text.Json.Serialization;
using Peers.Core.Cqrs.Pipeline;

namespace Peers.Modules.Lookup.Commands;

public static class LinkLookupOptions
{
    /// <summary>
    /// Establishes links between a parent option and one or more child options of the specified type.
    /// </summary>
    /// <param name="LookupTypeKey">The unique key of the lookup type to which this entry belongs. Must correspond to a valid catalog lookup type.</param>
    /// <param name="ChildLookupTypeKey">The unique key of the child lookup type whose options will be linked to the parent option.</param>
    /// <param name="ParentOptionCode">The code of the parent lookup option to which child options will be linked.</param>
    /// <param name="ChildOptionCodes">An array of codes representing the child lookup options to be linked to the parent option.</param>
    [Authorize(Roles = Roles.CatalogManager)]
    public sealed record Command(
        [property: JsonIgnore()] string LookupTypeKey,
        string ChildLookupTypeKey,
        string ParentOptionCode,
        string[] ChildOptionCodes) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.LookupTypeKey).NotEmpty();
            RuleFor(p => p.ChildLookupTypeKey).NotEmpty();
            RuleFor(p => p.ParentOptionCode).NotEmpty();
            RuleFor(p => p.ChildOptionCodes).NotEmpty();
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
                .Include(p => p.ParentLinks)
                .Include(p => p.Options.Where(p => p.Code == cmd.ParentOptionCode))
                .FirstAsync(p => p.Key == cmd.LookupTypeKey, ctk) is not { } parentLookupType)
            {
                return Result.BadRequest(detail: "Parent lookup type not found.");
            }

            if (await _context
                .LookupTypes
                .Include(p => p.Options.Where(p => cmd.ChildOptionCodes.Contains(p.Code)))
                .FirstAsync(p => p.Key == cmd.ChildLookupTypeKey, ctk) is not { } childLookupType)
            {
                return Result.BadRequest(detail: "Child lookup type not found.");
            }

            parentLookupType.LinkOptions(cmd.ParentOptionCode, childLookupType, cmd.ChildOptionCodes);
            await _context.SaveChangesAsync(ctk);
            return Result.Ok();
        }
    }
}
