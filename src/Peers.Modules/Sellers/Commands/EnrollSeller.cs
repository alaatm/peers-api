using Humanizer;
using Peers.Core.Nafath;
using Peers.Core.Nafath.Configuration;
using Peers.Core.Nafath.Models;
using E = Peers.Modules.Sellers.SellersErrors;

namespace Peers.Modules.Sellers.Commands;

public static class EnrollSeller
{
    /// <summary>
    /// Requests enrollment as a seller using a National ID via Nafath.
    /// </summary>
    /// <param name="NationalId">
    /// The National ID associated with the logged in user.
    /// Must be exactly 10 digits and starts with either "1" or "2".
    /// </param>
    [Authorize(Roles = Roles.Customer)]
    public sealed record Command(string NationalId) : LocalizedCommand;

    public sealed class Validator : AbstractValidator<Command>
    {
        private static readonly string _nationalId = nameof(Command.NationalId).Humanize();

        public Validator([NotNull] IStrLoc l)
            => RuleFor(p => p.NationalId)
                .Matches(RegexStatic.NationalIdRegex())
                .WithMessage(l["Must be exactly 10 digits and starts with either \"1\" or \"2\"."])
                .WithName(l[_nationalId]);
    }

    /// <summary>
    /// Returns a **Nafath enrollment code** that the user must use to complete enrollment.
    ///
    /// ### Enrollment flow
    ///
    /// 1. The client app should redirect the user to the **Nafath App** to complete enrollment.
    /// 2. After the user completes enrollment, the backend sends a **push notification**
    ///    to the client app with the enrollment result.
    /// 3. If enrollment succeeds:
    ///    - Sign the user out
    ///    - Prompt them to sign in again to refresh their authentication state
    ///
    /// **Important:** The user must re-authenticate after enrollment so the updated Nafath
    /// status is reflected in all subsequent requests.
    /// </summary>
    /// <param name="Random">The enrollment code returned by Nafath.</param>
    public sealed record Response(string Random);

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly NafathConfig _nafathConfig;
        private readonly INafathService _nafath;
        private readonly IServiceProvider _serviceProvider;
        private readonly IIdentityInfo _identity;
        private readonly ILogger<Handler> _log;
        private readonly IStrLoc _l;

        public Handler(
            NafathConfig nafathConfig,
            INafathService nafath,
            IServiceProvider serviceProvider,
            IIdentityInfo identity,
            ILogger<Handler> log,
            IStrLoc l)
        {
            _nafathConfig = nafathConfig;
            _nafath = nafath;
            _serviceProvider = serviceProvider;
            _identity = identity;
            _log = log;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            if (_nafathConfig.Bypass)
            {
                var nafathIdentity = new NafathIdentity(
                    cmd.NationalId,
                    FirstNameAr: "Bypass",
                    LastNameAr: "Bypass",
                    FirstNameEn: "Bypass",
                    LastNameEn: "Bypass",
                    null);

                await NafathCallback.Handler(_serviceProvider, _identity.Id, nafathIdentity);
                return Result.Ok(new Response("BYPASS"));
            }

            try
            {
                var response = await _nafath.SendRequestAsync(cmd.Lang, _identity.Id, cmd.NationalId, ctk);
                return Result.Ok(new Response(response.Random));
            }
            catch (NafathException ex)
            {
                if (ex.ErrorObject?.IsInvalidRequestData is true)
                {
                    return Result.BadRequest(_l[E.InvalidNafathEnrollmentRequest]);
                }

                _log.NafathRequestError(ex, _identity.Id, cmd.NationalId);
                return Result.Problem(_l[E.NafathEnrollmentError]);
            }
        }
    }
}
