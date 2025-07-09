namespace Mashkoor.Modules;

[ExcludeFromCodeCoverage]
internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Warning, "Login attempt from a non-existing user: {Username}.", SkipEnabledCheck = true)]
    public static partial void NoAccountLogin(this ILogger logger, string username);

    [LoggerMessage(LogLevel.Warning, "A device crash report has been sent. Silent flag: {Silent}", SkipEnabledCheck = true)]
    public static partial void DeviceCrashReported(this ILogger logger, bool silent);

    [LoggerMessage(LogLevel.Error, "Could not generate email verification link", SkipEnabledCheck = true)]
    public static partial void EmailVerificationLinkGenerationFailed(this ILogger logger);

    [LoggerMessage(LogLevel.Warning, "Invalid device registration tracking request, trackingId: {TrackingId}, deviceId: {DeviceId}. Tracking id not found.", SkipEnabledCheck = true)]
    public static partial void DeviceRegisterTrackingInvalidTracking(this ILogger logger, string trackingId, Guid deviceId);

    [LoggerMessage(LogLevel.Warning, "Invalid device registration tracking request, trackingId: {TrackingId}, deviceId: {DeviceId}. Old device id {OldDeviceId} not found.", SkipEnabledCheck = true)]
    public static partial void DeviceRegisterTrackingNonExistingDevice(this ILogger logger, string trackingId, Guid deviceId, Guid oldDeviceId);

    [LoggerMessage(LogLevel.Warning, "Invalid device registration tracking request, trackingId: {TrackingId}, deviceId: {DeviceId}, oldDeviceId: {OldDeviceId}. PNS handle needs update.", SkipEnabledCheck = true)]
    public static partial void DeviceRegisterTrackingSameToken(this ILogger logger, string trackingId, Guid deviceId, Guid oldDeviceId);

    [LoggerMessage(LogLevel.Warning, "Invalid device registration request. A tracking request was not expected, trackingId: {TrackingId}.", SkipEnabledCheck = true)]
    public static partial void DeviceRegisterUnexpectedTracking(this ILogger logger, string trackingId);

    [LoggerMessage(LogLevel.Warning, "Client version: {Version} parsing failed.", SkipEnabledCheck = true)]
    public static partial void ClientVersionParseFailed(this ILogger logger, string version);
}
