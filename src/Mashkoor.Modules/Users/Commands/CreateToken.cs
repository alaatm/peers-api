using System.Diagnostics;
using Humanizer;
using Mashkoor.Core.Cqrs.Pipeline;
using Mashkoor.Core.Data.Identity;
using Mashkoor.Core.Security.Jwt;
using Mashkoor.Core.Security.Totp;
using Mashkoor.Modules.Users.Commands.Responses;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Users.Commands;

public static class CreateToken
{
    internal static bool RefreshTokenTests { get; set; }

    /// <summary>
    /// The grant type.
    /// </summary>
    public enum GrantType
    {
        /// <summary>
        /// Multi factor grant request.
        /// </summary>
        Mfa,
        /// <summary>
        /// Password grant request.
        /// </summary>
        Password,
        /// <summary>
        /// Refresh token grant request.
        /// </summary>
        RefreshToken,
    }

    /// <summary>
    /// The command.
    /// </summary>
    /// <param name="Username">The username.</param>
    /// <param name="Password">The password.</param>
    /// <param name="GrantType">The grant type.</param>
    public sealed record Command(
        string Username,
        string Password,
        GrantType GrantType) : ICommand, IValidatable;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _grantType = nameof(Command.GrantType).Humanize();
        private static readonly string _username = nameof(Command.Username).Humanize();
        private static readonly string _password = nameof(Command.Password).Humanize();

        public Validator([NotNull] IStrLoc l)
        {
            RuleFor(p => p.GrantType).IsInEnum().WithName(l[_grantType]);
            RuleFor(p => p.Username).NotEmpty().WithName(l[_username]);
            RuleFor(p => p.Password).NotEmpty().WithName(l[_password]);
        }
    }

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly MashkoorContext _context;
        private readonly IdentityUserManager<AppUser, MashkoorContext> _userManager;
        private readonly TimeProvider _timeProvider;
        private readonly IJwtProvider _jwtProvider;
        private readonly ITotpTokenProvider _totpProvider;
        private readonly ILogger<Handler> _log;
        private readonly IStrLoc _l;

        public Handler(
            MashkoorContext context,
            IdentityUserManager<AppUser, MashkoorContext> userManager,
            TimeProvider timeProvider,
            IJwtProvider jwtProvider,
            ITotpTokenProvider totpProvider,
            ILogger<Handler> log,
            IStrLoc l)
        {
            _context = context;
            _userManager = userManager;
            _timeProvider = timeProvider;
            _jwtProvider = jwtProvider;
            _totpProvider = totpProvider;
            _log = log;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            var errorMessage = cmd.GrantType switch
            {
                GrantType.Mfa => _l["Invalid verification code."],
                GrantType.Password => _l["Username or password incorrect."],
                GrantType.RefreshToken => _l["Unable to refresh session."],
                _ => throw new UnreachableException(),
            };

            if (await _context.Users
                .Include(p => p.RefreshTokens.Where(p => p.Revoked == null))
                .FirstOrDefaultAsync(p => p.UserName == cmd.Username, ctk) is not AppUser user)
            {
                // Return unauthorized when not found to prevent checking account existence.
                return Result.Unauthorized(errorMessage);
            }

            if (user.Status is UserStatus.Banned)
            {
                return Result.Forbidden(_l["Access is forbidden."]);
            }

            RefreshToken? refreshToken = null;

            var authenticationSucceeded = cmd.GrantType switch
            {
                GrantType.Mfa => _totpProvider.Validate(cmd.Password, user, TotpPurpose.SignInPurpose),
                GrantType.Password => await _userManager.CheckPasswordAsync(user, cmd.Password),
                GrantType.RefreshToken => user.TryRefreshToken(_timeProvider.UtcNow(), cmd.Password, out refreshToken),
                _ => throw new UnreachableException(),
            };

            // TODO: fix this temp hack
            if (RefreshTokenTests)
            {
#pragma warning disable CA1848 // This is temporary and will be removed.
                _log.LogWarning("Test flag is set.");
#pragma warning restore CA1848 // Use the LoggerMessage delegates
                await Task.Delay(250, ctk);
            }

            if (authenticationSucceeded)
            {
                var (roles, claims) = await _userManager.GetRolesAndClaimsAsync(user);
                var (token, tokenExpiry) = _jwtProvider.BuildToken(roles, claims);
                refreshToken ??= user.GetOrCreateRefreshToken(_timeProvider.UtcNow());

                try
                {
                    await _context.SaveChangesAsync(ctk);
                    return Result.Ok(new JwtResponse(user.Firstname, cmd.Username, token, refreshToken.Token, tokenExpiry, roles));
                }
                catch (DbUpdateConcurrencyException)
                {
                    Debug.Assert(cmd.GrantType is GrantType.RefreshToken);
                    // The cause of this is multiple refresh token requests coming in at the same time.
                    //
                    // REQ1 -> creates new refresh token -> revokes current -> save -> done
                    // REQ2 -> creates new refresh token -> revokes current -> save -> concurrency error
                    //
                    // At this point a new refresh token should have been generated and saved in the database
                    // so we can just return this newly created active refresh token.
                    //

                    _context.ChangeTracker.Clear();
                    user = await _context.Users
                        .AsNoTracking()
                        .Include(p => p.RefreshTokens.Where(p => p.Revoked == null))
                        .FirstAsync(p => p.Id == user.Id, ctk);
                    refreshToken = user.GetActiveRefreshToken();

                    return Result.Ok(new JwtResponse(user.Firstname, cmd.Username, token, refreshToken.Token, tokenExpiry, roles));
                }
            }

            return Result.Unauthorized(errorMessage);
        }
    }
}
