using Humanizer;
using Mashkoor.Core.Cqrs.Pipeline;
using Mashkoor.Modules.Users.Queries;

namespace Mashkoor.Modules.Users.Commands;

public static class SetProfile
{
    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="Firstname">The first name.</param>
    /// <param name="Lastname">The last name.</param>
    /// <param name="Email">The email address.</param>
    /// <param name="PreferredLanguage">The preferred language.</param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Command(
        string? Firstname,
        string? Lastname,
        string? Email,
        string? PreferredLanguage) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _email = nameof(Command.Email).Humanize();

        public Validator([NotNull] IStrLoc l)
            => RuleFor(p => p.Email)
            .EmailAddress()
            .When(p => p.Email is not null)
            .WithName(l[_email]);
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly MashkoorContext _context;
        private readonly IIdentityInfo _identity;
        private readonly IStrLoc _l;

        public Handler(
            MashkoorContext context,
            IIdentityInfo identity,
            IStrLoc l)
        {
            _context = context;
            _identity = identity;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            var user = await _context
                .Users
                .FirstAsync(p => p.Id == _identity.Id, ctk);

            if (cmd.Email is not null)
            {
                var exists = await _context
                    .Users
                    .AnyAsync(p => p.Id != _identity.Id && p.Email == cmd.Email.Trim(), ctk);

                if (exists)
                {
                    return Result.Conflict(_l["Email address already taken."]);
                }
            }

            user.SetProfile(
                cmd.Firstname,
                cmd.Lastname,
                cmd.Email,
                cmd.PreferredLanguage);

            await _context.SaveChangesAsync(ctk);

            return Result.Ok(new GetProfile.Response(
                user.Firstname,
                user.Lastname,
                user.PhoneNumber ?? "",
                user.UpdatedEmail ?? user.Email,
                user.EmailConfirmed,
                user.PreferredLanguage
            ));
        }
    }
}
