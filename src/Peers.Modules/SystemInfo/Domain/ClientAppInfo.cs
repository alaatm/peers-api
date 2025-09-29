namespace Peers.Modules.SystemInfo.Domain;

/// <summary>
/// Represents a client application information.
/// </summary>
public sealed class ClientAppInfo : Entity
{
    /// <summary>
    /// The package name of the app.
    /// </summary>
    public string PackageName { get; set; } = default!;
    /// <summary>
    /// The hash string of the app.
    /// </summary>
    public string HashString { get; set; } = default!;
    /// <summary>
    /// The Android store link.
    /// </summary>
    public string AndroidStoreLink { get; set; } = default!;
    /// <summary>
    /// The iOS store link.
    /// </summary>
    public string IOSStoreLink { get; set; } = default!;
    /// <summary>
    /// The latest version of the app.
    /// </summary>
    public ClientAppVersion LatestVersion { get; set; } = default!;

    /// <summary>
    /// Gets the app store link for the specified platform.
    /// </summary>
    /// <param name="platform">The platform.</param>
    /// <returns></returns>
    public string? GetStoreLink([NotNull] string platform) => platform.ToUpperInvariant() switch
    {
        "ANDROID" => AndroidStoreLink,
        "IOS" => IOSStoreLink,
        _ => null,
    };
}

public sealed class ClientAppVersion
{
    /// <summary>
    /// The major version.
    /// </summary>
    public int Major { get; set; }
    /// <summary>
    /// The minor version.
    /// </summary>
    public int Minor { get; set; }
    /// <summary>
    /// The build number.
    /// </summary>
    public int Build { get; set; }
    /// <summary>
    /// The revision number.
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// The version string.
    /// </summary>
    public string VersionString => $"{Major}.{Minor}.{Build} ({Revision})";
    /// <summary>
    /// The version number.
    /// </summary>
    public Version Version => new(Major, Minor, Build, Revision);
}
