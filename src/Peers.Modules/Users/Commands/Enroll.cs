using Humanizer;
using Peers.Core.Background;
using Peers.Core.Cqrs.Pipeline;
using Peers.Modules.Users.Events;

namespace Peers.Modules.Users.Commands;

public static class Enroll
{
    /// <summary>
    /// Request to enroll a new user.
    /// </summary>
    /// <param name="Username">The username.</param>
    /// <param name="PhoneNumber">The phone number.</param>
    /// <param name="Platform">The platform (iOS or Android)</param>
    public sealed record Command(
        string Username,
        string PhoneNumber,
        string? Platform) : LocalizedCommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _username = nameof(Command.Username).Humanize();
        private static readonly string _phoneNumber = nameof(Command.PhoneNumber).Humanize();

        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.Username)
                .Username(l)
                .WithName(l[_username]);

            RuleFor(p => p.PhoneNumber)
                .PhoneNumber(l)
                .WithName(l[_phoneNumber]);
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;
        private readonly IIdentityInfo _identity;
        private readonly IProducer _producer;
        private readonly IStrLoc _l;

        public Handler(
            PeersContext context,
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

            var normalizedUsername = cmd.Username.Trim();
            var normalizedPhoneNumber = cmd.PhoneNumber.Trim();

            if (await _context.Users.AnyAsync(p =>
                p.UserName == normalizedUsername ||
                p.PhoneNumber == normalizedPhoneNumber, ctk))
            {
                return Result.Conflict(_l["Username or phone number already exist."]);
            }

            await _producer.PublishAsync(new EnrollRequested(
                _identity,
                normalizedUsername,
                normalizedPhoneNumber,
                cmd.Lang), ctk);

            return Result.Accepted();
        }
    }
}
