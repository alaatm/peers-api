using Humanizer;
using Mashkoor.Core.Background;
using Mashkoor.Core.Cqrs.Pipeline;
using Mashkoor.Modules.Users.Domain;
using Mashkoor.Modules.Users.Events;

namespace Mashkoor.Modules.Users.Commands;

public static class SignIn
{
    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="PhoneNumber">The phone number.</param>
    /// <param name="Platform">The platform (iOS or Android)</param>
    public sealed record Command(
        string PhoneNumber,
        string Platform) : LocalizedCommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _phoneNumber = nameof(Command.PhoneNumber).Humanize();

        public Validator([NotNull] IStrLoc l)
            => RuleFor(p => p.PhoneNumber).NotEmpty().PhoneNumber(l).WithName(l[_phoneNumber]);
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly MashkoorContext _context;
        private readonly IProducer _producer;
        private readonly IIdentityInfo _identity;
        private readonly ILogger<Handler> _log;
        private readonly IStrLoc _l;

        public Handler(
            MashkoorContext context,
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
            if (await _context
                .Users
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserName == cmd.PhoneNumber, ctk) is not AppUser user || user.IsDeleted)
            {
                _log.NoAccountLogin(cmd.PhoneNumber);
                return Result.BadRequest(_l["Account does not exist."]);
            }

            if (user.Status is UserStatus.Banned)
            {
                return Result.Forbidden(_l["Access is forbidden."]);
            }

            await _producer.PublishAsync(new SignInRequested(
                _identity,
                cmd.Platform,
                cmd.PhoneNumber,
                cmd.Lang), ctk);

            return Result.Accepted();
        }
    }
}
