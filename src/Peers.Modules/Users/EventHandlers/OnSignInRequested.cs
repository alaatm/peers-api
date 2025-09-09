using System.Globalization;
using Peers.Core.Communication.Sms;
using Peers.Core.Security.Totp;
using Peers.Modules.Users.Events;

namespace Peers.Modules.Users.EventHandlers;

public sealed class OnSignInRequested : INotificationHandler<SignInRequested>
{
    private readonly PeersContext _context;
    private readonly ITotpTokenProvider _totpProvider;
    private readonly ISmsService _sms;
    private readonly IStrLoc _l;

    public OnSignInRequested(
        PeersContext context,
        ITotpTokenProvider totpProvider,
        ISmsService sms,
        IStrLoc l)
    {
        _context = context;
        _totpProvider = totpProvider;
        _sms = sms;
        _l = l;
    }

    public async Task Handle([NotNull] SignInRequested notification, CancellationToken cancellationToken)
    {
        // Note: user is guaranteed to exist in a good form (i.e. not banned or deleted) from the calling endpoint.
        var user = await _context
            .Users
            .AsNoTracking()
            .FirstAsync(p => p.UserName == notification.Username, cancellationToken);

        var uiCulture = Thread.CurrentThread.CurrentUICulture;
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(notification.LangCode);

        if (_totpProvider.TryGenerate(user, TotpPurpose.SignInPurpose, out var otp))
        {
            await _sms.SendAsync(notification.Username, _l["Your Peers verification code is: {0}", otp]);
        }

        Thread.CurrentThread.CurrentUICulture = uiCulture;
    }
}
