using System.Globalization;
using Mashkoor.Core.Communication.Sms;
using Mashkoor.Core.Security.StrongKeys;
using Mashkoor.Core.Security.Totp.Configuration;
using Mashkoor.Modules.Users.Events;
using Microsoft.Extensions.Caching.Memory;

namespace Mashkoor.Modules.Users.EventHandlers;

public sealed class OnEnrollRequested : INotificationHandler<EnrollRequested>
{
    private readonly TotpConfig _config;
    private readonly IMemoryCache _cache;
    private readonly ISmsService _sms;
    private readonly IStrLoc _l;

    public OnEnrollRequested(
        TotpConfig config,
        IMemoryCache cache,
        ISmsService sms,
        IStrLoc l)
    {
        _config = config;
        _cache = cache;
        _sms = sms;
        _l = l;
    }

    public async Task Handle([NotNull] EnrollRequested notification, CancellationToken cancellationToken)
    {
        var uiCulture = Thread.CurrentThread.CurrentUICulture;
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(notification.LangCode);

        var otpExists = TryGetOrCreateOtp(notification, _config.Duration, out var otp);

        if (!otpExists)
        {
            await _sms.SendAsync(notification.Username, _l["Your Mashkoor verification code is: {0}", otp]);
        }

        Thread.CurrentThread.CurrentUICulture = uiCulture;
    }

    private bool TryGetOrCreateOtp(EnrollRequested notification, TimeSpan expiry, out string otp)
    {
        var otpExists = _cache.TryGetValue(notification.Username, out string? otpValue);
        otpValue ??= _cache.Set(notification.Username, _config.UseDefaultOtp ? _config.DefaultOtp : KeyGenerator.Create(4, true), expiry);

        otp = otpValue;
        return otpExists;
    }
}
