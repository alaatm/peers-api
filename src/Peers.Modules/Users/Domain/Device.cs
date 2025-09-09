namespace Peers.Modules.Users.Domain;

/// <summary>
/// Represents a system user's mobile device.
/// </summary>
public sealed class Device : Entity
{
    /// <summary>
    /// The user id.
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// The device id.
    /// </summary>
    public Guid DeviceId { get; set; } = default!;
    /// <summary>
    /// The device manufacturer.
    /// </summary>
    public string Manufacturer { get; set; } = default!;
    /// <summary>
    /// The device model.
    /// </summary>
    public string Model { get; set; } = default!;
    /// <summary>
    /// The device platform.
    /// </summary>
    public string Platform { get; set; } = default!;
    /// <summary>
    /// The OS version.
    /// </summary>
    public string OSVersion { get; set; } = default!;
    /// <summary>
    /// The idiom.
    /// </summary>
    public string Idiom { get; set; } = default!;
    /// <summary>
    /// The device type.
    /// </summary>
    public string DeviceType { get; set; } = default!;
    /// <summary>
    /// The PNS handle.
    /// </summary>
    public string? PnsHandle { get; set; } = default!;
    /// <summary>
    /// The timestamp when PNS handle was last updated.
    /// </summary>
    public DateTime PnsHandleTimestamp { get; set; }
    /// <summary>
    /// The timestamp when PNS handle was last refreshed regardless whether updated or not.
    /// </summary>
    public DateTime PnsHandleLastRefreshed { get; set; }
    /// <summary>
    /// The date and time when this device was registered.
    /// </summary>
    public DateTime RegisteredOn { get; set; }
    /// <summary>
    /// The app package name.
    /// </summary>
    public string App { get; set; } = default!;
    /// <summary>
    /// The app version.
    /// </summary>
    public string AppVersion { get; set; } = default!;
    /// <summary>
    /// The last time a ping was received from this device.
    /// </summary>
    public DateTime? LastPing { get; set; }
    /// <summary>
    /// The owning user.
    /// </summary>
    public AppUser User { get; set; } = default!;

    /// <summary>
    /// Returns whether or not device handle is stalled.
    /// </summary>
    /// <remarks>
    /// Client app should request pns handle update on every run
    /// even if no new handle is assigned by provider so that we
    /// know it is still active to avoid sending push notifications
    /// and get false delivery-failure reports.
    /// </remarks>
    public bool IsStalled(DateTime checkDate) => (checkDate - PnsHandleLastRefreshed).TotalDays > 61;

    private Device() { }

    internal Device(DateTime date, AppUser user, Guid deviceId, string manufacturer, string model, string platform, string oSVersion,
        string idiom, string deviceType, string pnsHandle, string app, string appVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(deviceId));
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(manufacturer));
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(model));
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(platform));
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(oSVersion));
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(idiom));
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(deviceType));
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(pnsHandle));
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(app));
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(appVersion));

        User = user;
        DeviceId = deviceId;
        Manufacturer = manufacturer;
        Model = model;
        Platform = platform;
        OSVersion = oSVersion;
        Idiom = idiom;
        DeviceType = deviceType;
        PnsHandle = pnsHandle;
        PnsHandleTimestamp = date;
        PnsHandleLastRefreshed = date;
        RegisteredOn = date;
        App = app;
        AppVersion = appVersion;
    }

    /// <summary>
    /// Updates device's pns handle as well handle's timestamp.
    /// </summary>
    /// <param name="date">The date when the handle was updated.</param>
    /// <param name="pnsHandle">The new handle, or null to remove it.</param>
    public void UpdateHandle(DateTime date, string? pnsHandle)
    {
        if (pnsHandle != PnsHandle)
        {
            PnsHandle = pnsHandle;
            PnsHandleTimestamp = date;
        }

        PnsHandleLastRefreshed = date;
    }

    /// <summary>
    /// Updates device's app version.
    /// </summary>
    /// <param name="appVersion">The app's version.</param>
    public void UpdateAppVersion(string appVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(appVersion));

        if (appVersion != AppVersion)
        {
            AppVersion = appVersion;
        }
    }
}
