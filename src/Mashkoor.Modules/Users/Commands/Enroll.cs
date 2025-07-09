using Humanizer;
using Mashkoor.Core.Background;
using Mashkoor.Core.Cqrs.Pipeline;
using Mashkoor.Modules.Users.Commands.Responses;
using Mashkoor.Modules.Users.Events;

namespace Mashkoor.Modules.Users.Commands;

public static class Enroll
{
    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="Username">The username.</param>
    /// <param name="Platform">The platform (iOS or Android)</param>
    public sealed record Command(
        string Username,
        string? Platform) : LocalizedCommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _username = nameof(Command.Username).Humanize();

        public Validator([NotNull] IStrLoc l)
            => RuleFor(p => p.Username)
                .NotEmpty()
                .PhoneNumber(l).WithName(l[_username]);
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly MashkoorContext _context;
        private readonly IIdentityInfo _identity;
        private readonly IProducer _producer;
        private readonly IStrLoc _l;

        public Handler(
            MashkoorContext context,
            IIdentityInfo identity,
            IProducer producer,
            IStrLoc l)
        {
            _context = context;
            _identity = identity;
            _producer = producer;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            if (_identity.IsAuthenticated)
            {
                return Result.BadRequest(_l["You are already authenticated."]);
            }

            if (await _context.Users.AnyAsync(p => p.UserName == cmd.Username.Trim(), ctk))
            {
                return Result.Conflict(_l["User already exist."]);
            }

            await _producer.PublishAsync(new EnrollRequested(
                _identity,
                cmd.Username,
                cmd.Lang), ctk);

            return Result.Accepted(value: new OtpResponse(cmd.Username));
        }
    }
}
