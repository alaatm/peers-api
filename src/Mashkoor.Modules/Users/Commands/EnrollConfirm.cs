using Humanizer;
using Mashkoor.Core.Cqrs.Pipeline;
using Mashkoor.Core.Data.Identity;
using Mashkoor.Core.Security.Hashing;
using Mashkoor.Core.Security.Jwt;
using Mashkoor.Modules.Customers.Domain;
using Mashkoor.Modules.Users.Commands.Responses;
using Mashkoor.Modules.Users.Domain;
using Microsoft.Extensions.Caching.Memory;

namespace Mashkoor.Modules.Users.Commands;

public static class EnrollConfirm
{
    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="Otp">The one time password.</param>
    /// <param name="Username">The username.</param>
    /// <param name="Password">The password (Used only for 'Password' enroll type).</param>
    /// <param name="FirstName">The first name.</param>
    /// <param name="LastName">The last name.</param>
    /// <param name="PreferredLanguage">The preferred language for this user.</param>
    public sealed record Command(
        string Otp,
        string Username,
        string? Password,
        string FirstName,
        string LastName,
        string PreferredLanguage) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _otp = nameof(Command.Otp).Humanize();
        private static readonly string _username = nameof(Command.Username).Humanize();
        private static readonly string _firstName = nameof(Command.FirstName).Humanize();
        private static readonly string _lastName = nameof(Command.LastName).Humanize();
        private static readonly string _preferredLanguage = nameof(Command.PreferredLanguage).Humanize();

        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.Otp).NotEmpty().Length(4, 4).WithName(l[_otp]);
            RuleFor(p => p.FirstName).NotEmpty().WithName(l[_firstName]);
            RuleFor(p => p.LastName).NotEmpty().WithName(l[_lastName]);
            RuleFor(p => p.PreferredLanguage).NotEmpty().MinimumLength(2).MaximumLength(5).WithName(l[_preferredLanguage]);
            RuleFor(p => p.Username).PhoneNumber(l).WithName(l[_username]);
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly MashkoorContext _context;
        private readonly IdentityUserManager<AppUser, MashkoorContext> _userManager;
        private readonly TimeProvider _timeProvider;
        private readonly IJwtProvider _jwtProvider;
        private readonly IHmacHash _hmacHash;
        private readonly IMemoryCache _cache;
        private readonly IIdentityInfo _identity;
        private readonly IStrLoc _l;

        public Handler(
            MashkoorContext context,
            IdentityUserManager<AppUser, MashkoorContext> userManager,
            TimeProvider timeProvider,
            IJwtProvider jwtProvider,
            IHmacHash hmacHash,
            IMemoryCache cache,
            IIdentityInfo identity,
            IStrLoc l)
        {
            _context = context;
            _userManager = userManager;
            _timeProvider = timeProvider;
            _jwtProvider = jwtProvider;
            _hmacHash = hmacHash;
            _cache = cache;
            _identity = identity;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            if (_identity.IsAuthenticated)
            {
                return Result.BadRequest(_l["You are already authenticated."]);
            }

            if (await _context.Users.AnyAsync(p => p.UserName == cmd.Username, ctk))
            {
                return Result.Conflict(_l["User already exist."]);
            }

            if (cmd.Otp != _cache.Get<string>(cmd.Username))
            {
                return Result.BadRequest(_l["Invalid verification code."]);
            }

            var userRoles = new string[] { Roles.Customer };
            var user = AppUser.CreateTwoFactorAccount(_timeProvider.UtcNow(), cmd.Username, cmd.FirstName, cmd.LastName, cmd.PreferredLanguage);
            var customer = Customer.Create(user, _hmacHash.GenerateKey());

            // CreateUserAsync will append additional claims (userId and username) and returns all claims.
            // Make sure to use the returned claims for building the JWT token.
            var userClaims = await _userManager.CreateUserAsync(false, user, userRoles, []);

            var (token, tokenExpiry) = _jwtProvider.BuildToken(userRoles, userClaims);
            var refreshToken = user.GetOrCreateRefreshToken(_timeProvider.UtcNow());

            // The AppUser was previously added to the context.
            // Here we attach the user. Doing it the other way around, i.e. inserting the user first (which already references the AppUser)
            // will cause EFCore to skip 1 id number when using HiLo which we are using for the AppUser entity.
            _context.Attach(customer);
            await _context.SaveChangesAsync(ctk);
            return Result.Ok(new JwtResponse(cmd.FirstName, cmd.Username, token, refreshToken.Token, tokenExpiry, userRoles));
        }
    }
}
