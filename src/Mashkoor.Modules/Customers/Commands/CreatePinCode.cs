using Humanizer;
using Mashkoor.Core.Cqrs.Pipeline;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Customers.Commands;

public static class CreatePinCode
{
    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="PinCode">The user's PIN.</param>
    /// <param name="PinCodeConfirmation">The user's PIN confirmation.</param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Command(
        string PinCode,
        string PinCodeConfirmation) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _pin = nameof(Command.PinCode).Humanize();
        private static readonly string _pinConfirmation = nameof(Command.PinCodeConfirmation).Humanize();

        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.PinCode).NotNull().NotEmpty().MinimumLength(6).MaximumLength(6).WithName(l[_pin]);
            RuleFor(p => p.PinCodeConfirmation).Equal(p => p.PinCode).WithName(l[_pinConfirmation]);
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly MashkoorContext _context;
        private readonly IIdentityInfo _identity;
        private readonly IPasswordHasher<AppUser> _passwordHasher;
        private readonly IStrLoc _l;

        public Handler(
            MashkoorContext userManager,
            IIdentityInfo identity,
            IPasswordHasher<AppUser> passwordHasher,
            IStrLoc l)
        {
            _context = userManager;
            _identity = identity;
            _passwordHasher = passwordHasher;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            var customer = await _context
                .Customers
                .Include(p => p.User)
                .FirstAsync(p => p.Id == _identity.Id, ctk);

            if (customer.PinCodeHash is not null)
            {
                return Result.BadRequest(_l["PIN code already exists for this account."]);
            }

            customer.PinCodeHash = _passwordHasher.HashPassword(customer.User, cmd.PinCode);
            await _context.SaveChangesAsync(ctk);
            return Result.Created();
        }
    }
}
