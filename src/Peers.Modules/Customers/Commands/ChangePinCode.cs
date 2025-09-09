using Humanizer;
using Peers.Core.Cqrs.Pipeline;
using Peers.Modules.Kernel.Startup;
using Peers.Modules.Users.Domain;
using Microsoft.Extensions.Caching.Memory;

namespace Peers.Modules.Customers.Commands;

public static class ChangePinCode
{
    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="CurrentPinCode">The user's current PIN.</param>
    /// <param name="PinCode">The user's PIN.</param>
    /// <param name="PinCodeConfirmation">The user's PIN confirmation.</param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Command(
        string CurrentPinCode,
        string PinCode,
        string PinCodeConfirmation) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _oldPin = nameof(Command.CurrentPinCode).Humanize();
        private static readonly string _pin = nameof(Command.PinCode).Humanize();
        private static readonly string _pinConfirmation = nameof(Command.PinCodeConfirmation).Humanize();

        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.CurrentPinCode).NotEmpty().MinimumLength(6).MaximumLength(6).WithName(l[_oldPin]);
            RuleFor(p => p.PinCode).NotEmpty().MinimumLength(6).MaximumLength(6).NotEqual(p => p.CurrentPinCode).WithName(l[_pin]);
            RuleFor(p => p.PinCodeConfirmation).Equal(p => p.PinCode).WithName(l[_pinConfirmation]);
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly PeersContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly IIdentityInfo _identity;
        private readonly IPasswordHasher<AppUser> _passwordHasher;
        private readonly IMemoryCache _cache;
        private readonly IStrLoc _l;

        public Handler(
            PeersContext userManager,
            TimeProvider timeProvider,
            IIdentityInfo identity,
            IPasswordHasher<AppUser> passwordHasher,
            IMemoryCache cache,
            IStrLoc l)
        {
            _context = userManager;
            _timeProvider = timeProvider;
            _identity = identity;
            _passwordHasher = passwordHasher;
            _cache = cache;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            var customer = await _context
                .Customers
                .Include(p => p.User)
                .FirstAsync(p => p.Id == _identity.Id, ctk);

            if (customer.User.Status is UserStatus.Suspended)
            {
                return Result.BadRequest(_l["Your account is suspended."]);
            }

            if (customer.PinCodeHash is null)
            {
                return Result.BadRequest(_l["You do not have a PIN code set."]);
            }

            if (_passwordHasher.VerifyHashedPassword(
                customer.User,
                customer.PinCodeHash,
                cmd.CurrentPinCode) is not PasswordVerificationResult.Success)
            {
                // Suspend user after 5 failed attempts within 1 hour.
                var attempts = _cache.GetOrCreate($"pin_attempts_{_identity.Id}", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return 0;
                });
                attempts++;

                _cache.Set($"pin_attempts_{_identity.Id}", attempts);
                if (attempts >= 5)
                {
                    // Suspend the user account
                    var admin = await _context.Users.FirstAsync(p => p.UserName == StartupBackgroundService.AdminUsername, ctk);
                    customer.User.ChangeStatus(_timeProvider.UtcNow(), admin, UserStatus.Suspended, "5 failed PIN code change attempts within one hour.");
                    await _context.SaveChangesAsync(ctk);
                    return Result.BadRequest(_l["Your account has been suspended due to too many failed attempts."]);
                }

                return Result.BadRequest(_l["Current PIN code is incorrect."]);
            }

            customer.PinCodeHash = _passwordHasher.HashPassword(customer.User, cmd.PinCode);
            await _context.SaveChangesAsync(ctk);
            return Result.NoContent();
        }
    }
}
