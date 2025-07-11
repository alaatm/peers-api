using Mashkoor.Core.Data;
using Mashkoor.Core.Localization;
using Mashkoor.Modules.Users.Rules;

namespace Mashkoor.Modules.Users.Domain;

/// <summary>
/// Represents a system user.
/// </summary>
public sealed class AppUser : IdentityUserBase, IAggregateRoot
{
    /// <summary>
    /// The registration date.
    /// </summary>
    public DateTime RegisteredOn { get; set; }
    /// <summary>
    /// The name to be displayed to identify this user.
    /// </summary>
    public string DisplayName { get; set; } = default!;
    /// <summary>
    /// The first name.
    /// </summary>
    /// <value></value>
    public string Firstname { get; set; } = default!;
    /// <summary>
    /// The last name.
    /// </summary>
    /// <value></value>
    public string? Lastname { get; set; }
    /// <summary>
    /// The user's image url.
    /// </summary>
#pragma warning disable CA1056 // URI-like properties should not be strings
    public string? ImageUrl { get; set; }
#pragma warning restore CA1056 // URI-like properties should not be strings
    /// <summary>
    /// The preferred communication language for this user.
    /// </summary>
    /// <remarks>
    /// Used for determining which language to use when sending push notifications, emails, etc.
    /// The device localization does not depend on this but rather on the device's locale
    /// </remarks>
    public string PreferredLanguage { get; set; } = default!;
    /// <summary>
    /// The current user status.
    /// </summary>
    public UserStatus Status { get; set; }
    /// <summary>
    /// The temporary email address to be set once confirmed.
    /// </summary>
    public string? UpdatedEmail { get; set; }
    /// <summary>
    /// Indicates whether this account has been deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
    /// <summary>
    /// For deleted accounts, the date when the account was deleted.
    /// </summary>
    public DateTime? DeletedOn { get; set; }
    /// <summary>
    /// For deleted accounts, the username before deletion.
    /// </summary>
    public string? OriginalDeletedUsername { get; set; }
    /// <summary>
    /// The banning change history for this user.
    /// </summary>
    public ICollection<UserStatusChange> StatusChangeHistory { get; set; } = default!;
    /// <summary>
    /// List of acquired refresh tokens.
    /// </summary>
    /// <value></value>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = default!;
    /// <summary>
    /// List of registered devices for this user.
    /// </summary>
    /// <value></value>
    public ICollection<Device> DeviceList { get; set; } = default!;
    /// <summary>
    /// List of notifications for this user.
    /// </summary>
    /// <value></value>
    public ICollection<UserNotification> Notifications { get; set; } = default!;
    /// <summary>
    /// A list of app usage history for this user.
    /// </summary>
    /// <remarks>
    /// Recorded each time the user opens the app.
    /// </remarks>
    public ICollection<AppUsageHistory> AppUsage { get; set; } = default!;

    /// <summary>
    /// Creates an account that supports two factor login via SMS.
    /// </summary>
    /// <param name="date">The date when the user registered.</param>
    /// <param name="phoneNumber">The phone number.</param>
    /// <param name="firstname">The first name.</param>
    /// <param name="lastname">The last name.</param>
    /// <param name="preferredLanguage">The preferred language for this user.</param>
    /// <returns></returns>
    public static AppUser CreateTwoFactorAccount(
        DateTime date,
        string phoneNumber,
        string firstname,
        string? lastname,
        string preferredLanguage)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstname);
        ArgumentException.ThrowIfNullOrWhiteSpace(preferredLanguage);
        if (!RegexStatic.PhoneNumberRegex().IsMatch(phoneNumber))
        {
            throw new ArgumentException("Invalid phone number format.", nameof(phoneNumber));
        }

        return new()
        {
            Status = UserStatus.Active,
            RegisteredOn = date,
            UserName = phoneNumber,
            Firstname = firstname.Trim(),
            Lastname = lastname?.Trim(),
            PreferredLanguage = preferredLanguage.Trim(),
            PhoneNumber = phoneNumber,
            PhoneNumberConfirmed = true,
            TwoFactorEnabled = true,
            RefreshTokens = [],
            DeviceList = [],
        };
    }

    /// <summary>
    /// Creates an account that supports username/password login.
    /// </summary>
    /// <param name="date">The date when the user registered.</param>
    /// <param name="emailAddress">The email address.</param>
    /// <param name="firstname">The first name.</param>
    /// <param name="lastname">The last name.</param>
    /// <param name="preferredLanguage">The preferred language for this user.</param>
    /// <returns></returns>
    public static AppUser CreatePasswordAccount(
        DateTime date,
        string emailAddress,
        string firstname,
        string lastname,
        string preferredLanguage)
    {
        ArgumentNullException.ThrowIfNull(emailAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstname);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastname);
        ArgumentException.ThrowIfNullOrWhiteSpace(preferredLanguage);
        if (!RegexStatic.EmailRegex().IsMatch(emailAddress))
        {
            throw new ArgumentException("Invalid email address format.", nameof(emailAddress));
        }

        return new()
        {
            Status = UserStatus.Active,
            RegisteredOn = date,
            UserName = emailAddress,
            Firstname = firstname,
            Lastname = lastname,
            PreferredLanguage = preferredLanguage.Trim(),
            Email = emailAddress,
            EmailConfirmed = true,
            RefreshTokens = [],
        };
    }

    /// <summary>
    /// Creates a staff member account.
    /// </summary>
    /// <param name="date">The date when the user registered.</param>
    /// <param name="emailAddress">The email address.</param>
    /// <param name="phoneNumber">The phone number</param>
    /// <param name="firstname">The first name.</param>
    /// <param name="lastname">The last name.</param>
    /// <returns></returns>
    public static AppUser CreateStaffAccount(
        DateTime date,
        string emailAddress,
        string? phoneNumber,
        string firstname,
        string lastname)
    {
        var user = CreatePasswordAccount(date, emailAddress, firstname, lastname, Lang.EnLangCode);
        user.PhoneNumber = phoneNumber;
        user.PhoneNumberConfirmed = phoneNumber is not null;
        return user;
    }

    /// <summary>
    /// Returns the active refresh token for this user or throws if none exist.
    /// </summary>
    /// <returns></returns>
    public RefreshToken GetActiveRefreshToken() => RefreshTokens.Single(p => p.IsActive);

    /// <summary>
    /// Returns the currently active refresh token if found. Otherwise,
    /// creates and appends a new refresh token for this user.
    /// </summary>
    /// <param name="date">The date when the refresh token was created.</param>
    /// <returns></returns>
    public RefreshToken GetOrCreateRefreshToken(DateTime date)
    {
        if (RefreshTokens.SingleOrDefault(p => p.IsActive) is RefreshToken rt)
        {
            return rt;
        }

        var refreshToken = RefreshToken.Create(date);
        RefreshTokens.Add(refreshToken);
        return refreshToken;
    }

    /// <summary>
    /// Attempts to acquire a new refresh token.
    /// </summary>
    /// <param name="date"> The date when the refresh token was created.</param>
    /// <param name="token">The current refresh token.</param>
    /// <param name="refreshToken">The new refresh token.</param>
    /// <returns></returns>
    public bool TryRefreshToken(
        DateTime date,
        string token,
        [NotNullWhen(true)] out RefreshToken? refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        // For banned and deleted user, there wont be any active refresh tokens
        if (RefreshTokens.SingleOrDefault(p => p.IsActive && p.Token == token) is RefreshToken rt)
        {
            // Revoke supplied token
            rt.Revoked = date;

            refreshToken = RefreshToken.Create(date);
            RefreshTokens.Add(refreshToken);
            return true;
        }

        refreshToken = null;
        return false;
    }

    /// <summary>
    /// Adds the specified device details to the list of registered devices for this user.
    /// </summary>
    /// <param name="date">The date when the device was registered.</param>
    /// <param name="deviceId">The device id.</param>
    /// <param name="manufacturer">The device manufacturer.</param>
    /// <param name="model">The device model.</param>
    /// <param name="platform">The device platform.</param>
    /// <param name="version">The OS version.</param>
    /// <param name="idiom">The idiom.</param>
    /// <param name="deviceType">The device type.</param>
    /// <param name="pnsHandle">The PNS handle.</param>
    /// <param name="app">The app package name.</param>
    /// <param name="appVersion">The app version.</param>
    public Device RegisterDevice(
        DateTime date,
        Guid deviceId,
        string manufacturer,
        string model,
        string platform,
        string version,
        string idiom,
        string deviceType,
        string pnsHandle,
        string app,
        string appVersion)
    {
        var device = new Device(
            date, this,
            deviceId, manufacturer, model, platform,
            version, idiom, deviceType, pnsHandle,
            app, appVersion);
        DeviceList.Add(device);
        return device;
    }

    /// <summary>
    /// Links the specified device to this user.
    /// </summary>
    /// <param name="device">The device.</param>
    public void LinkDevice([NotNull] Device device)
    {
        CheckRule(new CanLinkDeviceRule(this, device));
        device.User = this;
        DeviceList.Add(device);
    }

    /// <summary>
    /// Unlinks the specified device from this user.
    /// </summary>
    /// <param name="device">The device.</param>
    public void UnlinkDevice([NotNull] Device device)
    {
        CheckRule(new CanUnlinkDeviceRule(this, device));
        device.User = null!;
        DeviceList.Remove(device);
    }

    /// <summary>
    /// Updates user's preferred language.
    /// </summary>
    /// <param name="preferredLanguage">The user's preferred language.</param>
    public void SetPreferredLanguage(string preferredLanguage)
        => PreferredLanguage = preferredLanguage;

    /// <summary>
    /// Updates the PNS handle for the specified user device.
    /// </summary>
    /// <param name="date">The date when the PNS handle was updated.</param>
    /// <param name="deviceId">The device id.</param>
    /// <param name="pnsHandle">The new PNS handle or null to remove it.</param>
    /// <param name="oldHandle">The old PNS handle.</param>
    public bool UpdatePnsHandle(
        DateTime date,
        Guid deviceId,
        string? pnsHandle,
        out string? oldHandle)
    {
        if (deviceId == default)
        {
            throw new ArgumentException("Device ID cannot be empty.", nameof(deviceId));
        }

        if (DeviceList.SingleOrDefault(p => p.DeviceId == deviceId) is Device device)
        {
            oldHandle = device.PnsHandle;
            device.UpdateHandle(date, pnsHandle);
            return true;
        }

        oldHandle = null;
        return false;
    }

    /// <summary>
    /// Changes the current status of the user.
    /// </summary>
    /// <param name="date">When the change occurred.</param>
    /// <param name="changedBy">The manager who is performing the change or the owning user when deleting own account.</param>
    /// <param name="newStatus">The new status.</param>
    /// <param name="changeReason">The reason for the change.</param>
    public void ChangeStatus(
        DateTime date,
        AppUser changedBy,
        UserStatus newStatus,
        string changeReason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(changeReason);

        CheckRule(new CanChangeUserStatusRule(this, newStatus));

        (StatusChangeHistory ??= [])
            .Add(UserStatusChange.Create(
                date,
                changedBy,
                Status,
                newStatus,
                changeReason));

        Status = newStatus;

        if (newStatus is UserStatus.Banned or UserStatus.Deleted)
        {
            // Revoke active refresh token, if one exist.
            if (RefreshTokens.SingleOrDefault(p => p.IsActive) is RefreshToken rt)
            {
                rt.Revoked = date;
            }
        }
    }

    /// <summary>
    /// Adds a new notification for this user.
    /// </summary>
    /// <param name="notification">The notification.</param>
    public void AddNotification(Notification notification)
        => (Notifications ??= [])
            .Add(UserNotification.Create(this, notification));

    /// <summary>
    /// Sets customer profile info.
    /// </summary>
    /// <param name="firstname">The first name.</param>
    /// <param name="lastname">The last name.</param>
    /// <param name="email">The email address.</param>
    /// <param name="preferredLanguage">The preferred language.</param>
    public void SetProfile(string? firstname, string? lastname, string? email, string? preferredLanguage)
    {
        preferredLanguage = preferredLanguage?.Trim();
        if (preferredLanguage is not null &&
            preferredLanguage.Length != 2)
        {
            throw new ArgumentOutOfRangeException(nameof(preferredLanguage), "Preferred language must be a two-letter ISO code.");
        }

        InitiateEmailUpdate(email);
        Firstname = firstname?.Trim() ?? Firstname;
        Lastname = lastname?.Trim() ?? Lastname;
        DisplayName = Firstname;
        PreferredLanguage = preferredLanguage ?? PreferredLanguage;
    }

    /// <summary>
    /// Records that the app has been used by this user.
    /// </summary>
    /// <param name="date">The date when the app was used.</param>
    public void RecordAppUsed(DateTime date)
        => (AppUsage ??= []).Add(new(date));

    /// <summary>
    /// Deletes the user account.
    /// </summary>
    /// <param name="date">The date when the account was deleted.</param>
    internal void DeleteAccount(DateTime date)
    {
        if (IsDeleted)
        {
            return;
        }

        var suffix = $"{Guid.NewGuid()}_deleted";

        DeletedOn = date;
        IsDeleted = true;
        OriginalDeletedUsername = UserName;
        UserName = $"{UserName}-{suffix}";
        NormalizedUserName = $"{NormalizedUserName}-{suffix}";
        Email = Email is not null ? $"{Email}-{suffix}" : null;
        NormalizedEmail = NormalizedEmail is not null ? $"{NormalizedEmail}-{suffix}" : null;
        PhoneNumber = PhoneNumber is not null ? $"{PhoneNumber}-{suffix}" : null;

        // This will also revoke any active refresh tokens
        ChangeStatus(date, this, UserStatus.Deleted, "User deleted own account.");

        // Unlink all devices
        foreach (var device in DeviceList.ToArray())
        {
            UnlinkDevice(device);
        }
    }

    internal void InitiateEmailUpdate(string? email)
    {
        email = email?.Trim();

        if (email is not null)
        {
            if (!RegexStatic.EmailRegex().IsMatch(email))
            {
                throw new ArgumentException("Invalid email address format.", nameof(email));
            }

            var isSameAsCurrentEmail = string.Equals(email, Email, StringComparison.OrdinalIgnoreCase);
            var isSameAsPendingEmail = string.Equals(email, UpdatedEmail, StringComparison.OrdinalIgnoreCase);

            if (UpdatedEmail is not null)
            {
                UpdatedEmail = email;
            }
            else
            {
                if (isSameAsCurrentEmail)
                {
                    // Nothing to do.
                }
                else
                {
                    UpdatedEmail = email;
                    EmailConfirmed = false;
                }
            }
        }
    }
}
