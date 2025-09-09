using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Peers.Core.Communication.Email;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Users.Commands;

public static class SendEmailVerification
{
    /// <summary>
    /// The command.
    /// </summary>
    [Authorize]
    public sealed record Command() : ICommand;

    public sealed class Handler : ICommandHandler<Command>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly LinkGenerator _linkGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly IIdentityInfo _identity;
        private readonly ILogger<Handler> _log;
        private readonly IStrLoc _l;

        public Handler(
            UserManager<AppUser> userManager,
            LinkGenerator linkGenerator,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService,
            IIdentityInfo identity,
            ILogger<Handler> log,
            IStrLoc l)
        {
            _userManager = userManager;
            _linkGenerator = linkGenerator;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _identity = identity;
            _log = log;
            _l = l;
        }

        public async Task<IResult> Handle([NotNull] Command cmd, CancellationToken ctk)
        {
            var user = (await _userManager.FindByNameAsync(_identity.Username!))!;

            if (user.EmailConfirmed)
            {
                return Result.NoContent();
            }

            if (user.UpdatedEmail is null)
            {
                return Result.BadRequest(_l["Email address is not set."]);
            }

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, user.UpdatedEmail);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var url = _linkGenerator.GetUriByPage(
                _httpContextAccessor.HttpContext!,
                "/email/verify",
                null,
                new { username = user.UserName, token, culture = user.PreferredLanguage },
                options: new LinkOptions
                {
                    AppendTrailingSlash = true,
                    LowercaseUrls = true,
                });

            if (url is null)
            {
                _log.EmailVerificationLinkGenerationFailed();
                return Result.Problem(_l["Could not generate email verification link."]);
            }

            var msg = _l["""
                <p>
                  Hi <b>{0}</b>,
                </p>
                <p>
                  Someone, possibly you, has requested to update your Peers account email address.
                </p>
                <p>
                  Please review and confirm your <b>email address below within one day</b>.
                </p>
                <p>Email address: <b>{1}</b></p>
                <p>Phone number: <b>{2}</b></p>
                <br/>
                <a href='{3}'>Confirm email</a>
                """, user.Firstname, user.UpdatedEmail, user.PhoneNumber?.Replace("+", "", StringComparison.Ordinal) ?? "", url];

            await _emailService.SendAsync(
                _l["Please confirm your new email address"],
                msg,
                user.UpdatedEmail);

            return Result.NoContent();
        }
    }
}
