using Humanizer;
using Peers.Core.Cqrs.Pipeline;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Users.Commands;

public static class ChangePassword
{
    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="CurrentPassword">The user's current password.</param>
    /// <param name="NewPassword">The user's password.</param>
    /// <param name="NewPasswordConfirm">The company name.</param>
    [Authorize]
    public sealed record Command(
        string CurrentPassword,
        string NewPassword,
        string NewPasswordConfirm) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _currentPassword = nameof(Command.CurrentPassword).Humanize();
        private static readonly string _newPassword = nameof(Command.NewPassword).Humanize();
        private static readonly string _newPasswordConfirm = nameof(Command.NewPasswordConfirm).Humanize();

        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.CurrentPassword).NotEmpty().MinimumLength(6).WithName(l[_currentPassword]);
            RuleFor(p => p.NewPassword).NotEmpty().MinimumLength(6).NotEqual(p => p.CurrentPassword).WithName(l[_newPassword]);
            RuleFor(p => p.NewPasswordConfirm).Equal(p => p.NewPassword).WithName(l[_newPasswordConfirm]);
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IIdentityInfo _identity;
        private readonly IStrLoc _l;

        public Handler(
            UserManager<AppUser> userManager,
            IIdentityInfo identity,
            IStrLoc l)
        {
            _userManager = userManager;
            _identity = identity;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            var user = (await _userManager.FindByNameAsync(_identity.Username!))!;

            // Only password-type accounts can change passwords
            if (!await _userManager.HasPasswordAsync(user))
            {
                return Result.AccessRestricted(_l["You are not authorized to perform this operation."]);
            }

            if (await _userManager.ChangePasswordAsync(user, cmd.CurrentPassword, cmd.NewPassword) is { Succeeded: false } r)
            {
                return Result.BadRequest(
                    detail: _l["Password change failed"],
                    errors: [.. r.Errors.Select(e => _l[e.Description].Value)]);
            }
            else
            {
                return Result.NoContent();
            }
        }
    }
}
