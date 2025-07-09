using Humanizer;
using Mashkoor.Core.Communication.Sms;
using Mashkoor.Core.Cqrs.Pipeline;
using Mashkoor.Core.Localization;
using Mashkoor.Core.Security.Totp;
using Mashkoor.Modules.Users.Commands.Responses;
using Mashkoor.Modules.Users.Domain;

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
        string? Platform) : LocalizedCommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _phoneNumber = nameof(Command.PhoneNumber).Humanize();

        public Validator([NotNull] IStrLoc l)
            => RuleFor(p => p.PhoneNumber).NotEmpty().PhoneNumber(l).WithName(l[_phoneNumber]);
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly MashkoorContext _context;
        private readonly ISmsService _sms;
        private readonly ITotpTokenProvider _totpProvider;
        private readonly ILogger<Handler> _log;
        private readonly IStrLoc _l;

        public Handler(
            MashkoorContext context,
            ISmsService sms,
            ITotpTokenProvider totpProvider,
            ILogger<Handler> log,
            IStrLoc l)
        {
            _context = context;
            _sms = sms;
            _totpProvider = totpProvider;
            _log = log;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            if (await _context
                .Users
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserName == cmd.PhoneNumber, ctk) is not AppUser user)
            {
                _log.NoAccountLogin(cmd.PhoneNumber);
                // Return accepted even if user does not exist. We'll just not send any SMS.
                // This is to prevent user checking if account exist or not.
                return Result.Accepted(value: new OtpResponse(cmd.PhoneNumber));
            }

            if (user.Status is not UserStatus.Banned)
            {
                if (_totpProvider.TryGenerate(user, TotpPurpose.SignInPurpose, out var otp))
                {
                    var body = cmd.Lang switch
                    {
                        Lang.ArLangCode => $"رمز تحقق مشكور الخاص بك هو: {otp}",
                        _ => $"Your Mashkoor verification code is: {otp}"
                    };
                    await _sms.SendAsync(cmd.PhoneNumber, body);
                    return Result.Accepted(value: new OtpResponse(cmd.PhoneNumber));
                }
                else
                {
                    // A token was already generated and is still valid at the time
                    // of this request so don't send another SMS but return accepted result.
                    return Result.Accepted(value: new OtpResponse(cmd.PhoneNumber));
                }
            }

            return Result.Forbidden(_l["Access is forbidden."]);
        }
    }
}
