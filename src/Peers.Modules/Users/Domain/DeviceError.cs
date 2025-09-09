namespace Peers.Modules.Users.Domain;

public sealed class DeviceError : Entity
{
    /// <summary>
    /// The date of the report.
    /// </summary>
    /// <value></value>
    public DateTime ReportedOn { get; set; }
    /// <summary>
    /// The device id.
    /// </summary>
    /// <value></value>
    public Guid DeviceId { get; set; }
    /// <summary>
    /// The username on the device.
    /// </summary>
    /// <value></value>
    public string? Username { get; set; }
    /// <summary>
    /// The device's locale.
    /// </summary>
    /// <value></value>
    public string? Locale { get; set; }
    /// <summary>
    /// Indicates whether this is a silent error.
    /// </summary>
    public bool Silent { get; set; }
    /// <summary>
    /// The error source, can be either "Flutter", "Platform" or "Firebase".
    /// </summary>
    public string? Source { get; set; }
    /// <summary>
    /// The app version.
    /// </summary>
    public string? AppVersion { get; set; }
    /// <summary>
    /// The app state when this error occurred. Possible values are:
    /// - "resumed"
    /// - "inactive"
    /// - "paused"
    /// - "detached"
    /// - null
    /// </summary>
    /// <value></value>
    public string? AppState { get; set; }
    /// <summary>
    /// The exception.
    /// </summary>
    /// <value></value>
    public string? Exception { get; set; }
    /// <summary>
    /// The stack trace.
    /// </summary>
    /// <value></value>
    public string[] StackTrace { get; set; } = default!;
    /// <summary>
    /// Additional info regarding the error
    /// </summary>
    public string[] Info { get; set; } = default!;
    /// <summary>
    /// A json map of the device's properties.
    /// </summary>
    /// <value></value>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="DeviceError"/>.
    /// </summary>
    /// <param name="date">The date of the report.</param>
    /// <param name="deviceId"></param>
    /// <param name="username"></param>
    /// <param name="locale"></param>
    /// <param name="silent"></param>
    /// <param name="source"></param>
    /// <param name="appVersion"></param>
    /// <param name="appState"></param>
    /// <param name="exception"></param>
    /// <param name="stackTrace"></param>
    /// <param name="info"></param>
    /// <param name="deviceInfo"></param>
    /// <returns></returns>
    public static DeviceError Create(
        DateTime date,
        Guid deviceId,
        string? username,
        string? locale,
        bool silent,
        string? source,
        string? appVersion,
        string? appState,
        string? exception,
        string[] stackTrace,
        string[] info,
        string? deviceInfo) => new()
        {
            ReportedOn = date,
            DeviceId = deviceId,
            Username = username,
            Locale = locale,
            Silent = silent,
            Source = source,
            AppVersion = appVersion,
            AppState = appState,
            Exception = exception,
            StackTrace = stackTrace,
            Info = info,
            DeviceInfo = deviceInfo
        };
}
