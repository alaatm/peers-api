using Humanizer;
using Mashkoor.Core.Cqrs.Pipeline;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Users.Commands;

public static class ResetPasswordConfirm
{
    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="Otp">The reset password token.</param>
    /// <param name="Username">The username.</param>
    /// <param name="NewPassword">The new password.</param>
    public sealed record Command(
        string Otp,
        string Username,
        string NewPassword) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _otp = nameof(Command.Otp).Humanize();
        private static readonly string _username = nameof(Command.Username).Humanize();
        private static readonly string _newPassword = nameof(Command.NewPassword).Humanize();

        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.Otp).NotEmpty().Length(6, 6).WithName(l[_otp]);
            RuleFor(p => p.Username).NotEmpty().EmailAddress().WithName(l[_username]);
            RuleFor(p => p.NewPassword).NotEmpty().MinimumLength(6).WithName(l[_newPassword]);
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IStrLoc _l;

        public Handler(
            UserManager<AppUser> userManager,
            IStrLoc l)
        {
            _userManager = userManager;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            if (await _userManager.FindByNameAsync(cmd.Username) is AppUser user)
            {
                if (await _userManager.ResetPasswordAsync(user, cmd.Otp, cmd.NewPassword) is { Succeeded: false } r)
                {
                    return Result.BadRequest(
                        detail: _l["Password reset failed"],
                        errors: [.. r.Errors.Select(e => _l[e.Description].Value)]);
                }
                else
                {
                    return Result.NoContent();
                }
            }

            return Result.BadRequest(_l["User does not exist."]);
        }
    }
}
