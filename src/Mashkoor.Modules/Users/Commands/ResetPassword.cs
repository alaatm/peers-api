using Humanizer;
using Mashkoor.Core.Communication.Email;
using Mashkoor.Core.Cqrs.Pipeline;
using Mashkoor.Modules.Users.Commands.Responses;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Users.Commands;

public static class ResetPassword
{
    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="Username">The username.</param>
    public sealed record Command(string Username) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _username = nameof(Command.Username).Humanize();

        public Validator([NotNull] IStrLoc l)
            => RuleFor(p => p.Username).NotEmpty().EmailAddress().WithName(l[_username]);
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _email;
        private readonly IStrLoc _l;

        public Handler(
            UserManager<AppUser> userManager,
            IEmailService email,
            IStrLoc l)
        {
            _userManager = userManager;
            _email = email;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            if (await _userManager.FindByNameAsync(cmd.Username) is AppUser user)
            {
                var otp = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _email.SendAsync(_l["Password Reset"], _l["Your password reset code is: {0}", otp], user.Email!);
                return Result.Accepted(value: new OtpResponse(cmd.Username));
            }

            // Always return accepted even when not found to prevent
            // account existence checks.
            return Result.Accepted();
        }
    }
}
