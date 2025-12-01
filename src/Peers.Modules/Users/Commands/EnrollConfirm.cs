using System.Globalization;
using System.Text;
using Humanizer;
using Microsoft.Extensions.Caching.Memory;
using Peers.Core.Cqrs.Pipeline;
using Peers.Core.Data.Identity;
using Peers.Core.Security.Hashing;
using Peers.Core.Security.Jwt;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Users.Commands.Responses;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Users.Commands;

public static class EnrollConfirm
{
    /// <summary>
    /// Completes the enrollment of a new user.
    /// </summary>
    /// <param name="Otp">The one time password.</param>
    /// <param name="Username">The username.</param>
    /// <param name="PhoneNumber">The phone number.</param>
    /// <param name="Password">The password (Used only for 'Password' enroll type).</param>
    /// <param name="PreferredLanguage">The preferred language for this user.</param>
    public sealed record Command(
        string Otp,
        string Username,
        string PhoneNumber,
        string? Password,
        string PreferredLanguage) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _otp = nameof(Command.Otp).Humanize();
        private static readonly string _username = nameof(Command.Username).Humanize();
        private static readonly string _phoneNumber = nameof(Command.PhoneNumber).Humanize();
        private static readonly string _preferredLanguage = nameof(Command.PreferredLanguage).Humanize();

        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.Otp).NotEmpty().Length(4, 4).WithName(l[_otp]);
            RuleFor(p => p.Username)
                .Username(l)
                .WithName(l[_username]);

            RuleFor(p => p.PhoneNumber)
                .PhoneNumber(l)
                .WithName(l[_phoneNumber]);

            RuleFor(p => p.PreferredLanguage).NotEmpty().MinimumLength(2).MaximumLength(5).WithName(l[_preferredLanguage]);
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        internal static readonly CompositeFormat OtpCacheKeyFormat = CompositeFormat.Parse("{0}:{1}");

        private readonly PeersContext _context;
        private readonly IdentityUserManager<AppUser, PeersContext> _userManager;
        private readonly TimeProvider _timeProvider;
        private readonly IJwtProvider _jwtProvider;
        private readonly IHmacHash _hmacHash;
        private readonly IMemoryCache _cache;
        private readonly IIdentityInfo _identity;
        private readonly IStrLoc _l;

        public Handler(
            PeersContext context,
            IdentityUserManager<AppUser, PeersContext> userManager,
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

            var normalizedUsername = cmd.Username.Trim();
            var normalizedPhoneNumber = cmd.PhoneNumber.Trim();

            if (await _context.Users.AnyAsync(p =>
                p.UserName == normalizedUsername ||
                p.PhoneNumber == normalizedPhoneNumber, ctk))
            {
                return Result.Conflict(_l["Username or phone number already exist."]);
            }

            var cacheKey = string.Format(CultureInfo.InvariantCulture, OtpCacheKeyFormat, normalizedUsername, normalizedPhoneNumber);
            if (cmd.Otp != _cache.Get<string>(cacheKey))
            {
                return Result.BadRequest(_l["Invalid verification code."]);
            }

            var userRoles = new string[] { Roles.Customer };
            var user = AppUser.CreateTwoFactorAccount(_timeProvider.UtcNow(), normalizedUsername, normalizedPhoneNumber, cmd.PreferredLanguage);
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
            return Result.Ok(new JwtResponse(normalizedUsername, token, refreshToken.Token, tokenExpiry, userRoles));
        }
    }
}
