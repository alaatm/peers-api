using System.Diagnostics;
using Humanizer;
using Peers.Core.Background;
using Peers.Core.Cqrs.Pipeline;
using Peers.Modules.Users.Domain;
using Peers.Modules.Users.Events;

namespace Peers.Modules.Users.Commands;

public static class SignIn
{
    /// <summary>
    /// Request to sign in an existing user by either `username` or `phoneNumber`.
    /// </summary>
    /// <param name="Username">The username. If set, it will be used to sign in the user.</param>
    /// <param name="PhoneNumber">The phone number. Used to sign in the user if the `username` is not provided.</param>
    /// <param name="Platform">The platform (iOS or Android)</param>
    public sealed record Command(
        string? Username,
        string? PhoneNumber,
        string Platform) : LocalizedCommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _username = nameof(Command.Username).Humanize();
        private static readonly string _phoneNumber = nameof(Command.PhoneNumber).Humanize();

        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p)
                .Must(p => !string.IsNullOrWhiteSpace(p.Username) ||
                           !string.IsNullOrWhiteSpace(p.PhoneNumber))
                .WithMessage(l["Either 'username' or 'phoneNumber' must be provided."]);

            RuleFor(p => p.Username)
                .Username(l).WithName(l[_username])
                .When(x => !string.IsNullOrWhiteSpace(x.Username));

            RuleFor(p => p.PhoneNumber)
                .PhoneNumber(l).WithName(l[_phoneNumber])
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;
        private readonly IProducer _producer;
        private readonly IIdentityInfo _identity;
        private readonly ILogger<Handler> _log;
        private readonly IStrLoc _l;

        public Handler(
            PeersContext context,
            IProducer producer,
            IIdentityInfo identity,
            ILogger<Handler> log,
            IStrLoc l)
        {
            _context = context;
            _producer = producer;
            _identity = identity;
            _log = log;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            var q = _context.Users.AsNoTracking();

            if (cmd.Username is not null)
            {
                q = q.Where(p => p.UserName == cmd.Username);
            }
            else
            {
                Debug.Assert(cmd.PhoneNumber is not null);
                q = q.Where(p => p.PhoneNumber == cmd.PhoneNumber);
            }

            if (await q.FirstOrDefaultAsync(ctk) is not AppUser user || user.IsDeleted)
            {
                _log.NoAccountLogin((cmd.Username ?? cmd.PhoneNumber)!);
                return Result.BadRequest(_l["Account does not exist."]);
            }

            if (user.Status is UserStatus.Banned)
            {
                return Result.Forbidden(_l["Access is forbidden."]);
            }

            await _producer.PublishAsync(new SignInRequested(
                _identity,
                cmd.Platform,
                cmd.Username,
                cmd.PhoneNumber,
                cmd.Lang), ctk);

            return Result.Accepted();
        }
    }
}
