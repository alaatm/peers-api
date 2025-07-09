using System.Globalization;
using Mashkoor.Core.Communication.Sms;
using Mashkoor.Core.Security.StrongKeys;
using Mashkoor.Modules.Users.Events;
using Microsoft.Extensions.Caching.Memory;

namespace Mashkoor.Modules.Users.EventHandlers;

public sealed class OnEnrollRequested : INotificationHandler<EnrollRequested>
{
    private readonly IMemoryCache _cache;
    private readonly ISmsService _sms;
    private readonly IStrLoc _l;

    public OnEnrollRequested(
        IMemoryCache cache,
        ISmsService sms,
        IStrLoc l)
    {
        _cache = cache;
        _sms = sms;
        _l = l;
    }

    public async Task Handle([NotNull] EnrollRequested notification, CancellationToken cancellationToken)
    {
        var uiCulture = Thread.CurrentThread.CurrentUICulture;
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(notification.LangCode);

        var expiry = TimeSpan.FromMinutes(5);

        var otpExists = TryGetOrCreateOtp(notification, expiry, out var otp);

        if (!otpExists)
        {
            await _sms.SendAsync(notification.Username, _l["Your Mashkoor verification code is: {0}", otp]);
        }

        Thread.CurrentThread.CurrentUICulture = uiCulture;
    }

    private bool TryGetOrCreateOtp(EnrollRequested notification, TimeSpan expiry, out string otp)
    {
        var otpExists = _cache.TryGetValue(notification.Username, out string? otpValue);
        otpValue ??= _cache.Set(notification.Username, KeyGenerator.Create(4, true), expiry);

        otp = otpValue;
        return otpExists;
    }
}
