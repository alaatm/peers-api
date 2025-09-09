using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;

namespace Peers.Core.Localization;

/// <summary>
/// A string localizer that tracks missing keys.
/// </summary>
/// <typeparam name="T">The type to provide strings for.</typeparam>
public class TrackingStringLocalizer<T> : StringLocalizer<T>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMissingKeyTrackerService _missingKeyTracker;

    public TrackingStringLocalizer(
        IStringLocalizerFactory factory,
        IHttpContextAccessor httpContextAccessor,
        IMissingKeyTrackerService missingKeyTracker) : base(factory)
    {
        _httpContextAccessor = httpContextAccessor;
        _missingKeyTracker = missingKeyTracker;
    }

    /// <inheritdoc />
    public override LocalizedString this[string name] => Resolve(name);

    /// <inheritdoc />
    public override LocalizedString this[string name, params object[] arguments] => Resolve(name, arguments);

    private LocalizedString Resolve(string name, params object[] arguments)
    {
        var localizedString = base[name, arguments];

        if (localizedString.ResourceNotFound)
        {
            var feature = _httpContextAccessor.HttpContext?.Features.Get<IRequestCultureFeature>();
            var language = feature?.RequestCulture.Culture.Name;

            _missingKeyTracker.TrackMissingKey(name, language);
        }

        return localizedString;
    }
}
