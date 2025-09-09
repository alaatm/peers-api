using Peers.Core.Localization;
using Peers.Modules.Users.Domain;
using Peers.Modules.Users.Rules;

namespace Peers.Modules.Test.Users.Domain;

public class AppUserTests : DomainEntityTestBase
{
    [Fact]
    public void CreateTwoFactorAccount_throws_on_invalid_phoneNumber()
    {
        // Arrange, act & assert
        var ex = Assert.Throws<ArgumentException>(() => AppUser.CreateTwoFactorAccount(DateTime.UtcNow, "invalid", "firstname", "lastname", "en"));
        Assert.Equal("Invalid phone number format. (Parameter 'phoneNumber')", ex.Message);
    }

    [Fact]
    public void CreateTwoFactorAccount_creates_mfa_account()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var phoneNumber = TestPhoneNumber();
        var firstname = $" {new Bogus.DataSets.Name().FirstName()} ";
        var lastname = $" {new Bogus.DataSets.Name().LastName()} ";
        var lang = Lang.EnLangCode;

        // Act
        var user = AppUser.CreateTwoFactorAccount(date, phoneNumber, firstname, lastname, lang);

        // Assert
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal(phoneNumber, user.UserName);
        Assert.Equal(phoneNumber, user.PhoneNumber);
        Assert.True(user.PhoneNumberConfirmed);
        Assert.True(user.TwoFactorEnabled);
        Assert.Equal(date, user.RegisteredOn);
        Assert.Equal(firstname.Trim(), user.Firstname);
        Assert.Equal(lastname.Trim(), user.Lastname);
        Assert.Equal(lang, user.PreferredLanguage);

        Assert.Null(user.Email);
        Assert.False(user.EmailConfirmed);
    }

    [Fact]
    public void CreatePasswordAccount_throws_on_invalid_emailAddress()
    {
        // Arrange, act & assert
        var ex = Assert.Throws<ArgumentException>(() => AppUser.CreatePasswordAccount(DateTime.UtcNow, "invalid", "firstname", "lastname", "en"));
        Assert.Equal("Invalid email address format. (Parameter 'emailAddress')", ex.Message);
    }

    [Fact]
    public void CreatePasswordAccount_creates_pwd_account()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var emailAddress = new Bogus.DataSets.Internet().Email();
        var firstname = new Bogus.DataSets.Name().FirstName();
        var lastname = new Bogus.DataSets.Name().LastName();
        var lang = Lang.EnLangCode;

        // Act
        var user = AppUser.CreatePasswordAccount(date, emailAddress, firstname, lastname, lang);

        // Assert
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal(emailAddress, user.UserName);
        Assert.Equal(emailAddress, user.Email);
        Assert.True(user.EmailConfirmed);
        Assert.Equal(date, user.RegisteredOn);
        Assert.Equal(firstname.Trim(), user.Firstname);
        Assert.Equal(lastname.Trim(), user.Lastname);
        Assert.Equal(lang, user.PreferredLanguage);

        Assert.Null(user.PhoneNumber);
        Assert.False(user.TwoFactorEnabled);
    }

    [Theory]
    [InlineData("555555555")]
    [InlineData(null)]
    public void CreateStaffAccount_creates_staff_pwd_account(string phoneNumber)
    {
        // Arrange
        var date = DateTime.UtcNow;
        var emailAddress = new Bogus.DataSets.Internet().Email();
        var firstname = new Bogus.DataSets.Name().FirstName();
        var lastname = new Bogus.DataSets.Name().LastName();

        // Act
        var user = AppUser.CreateStaffAccount(date, emailAddress, phoneNumber, firstname, lastname);

        // Assert
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal(emailAddress, user.UserName);
        Assert.Equal(emailAddress, user.Email);
        Assert.True(user.EmailConfirmed);
        Assert.Equal(date, user.RegisteredOn);
        Assert.Equal(firstname.Trim(), user.Firstname);
        Assert.Equal(lastname.Trim(), user.Lastname);
        Assert.Equal(Lang.EnLangCode, user.PreferredLanguage);

        Assert.Equal(phoneNumber, user.PhoneNumber);
        Assert.Equal(phoneNumber is not null, user.PhoneNumberConfirmed);
        Assert.False(user.TwoFactorEnabled);
    }

    [Fact]
    public void RegisterDevice_registers_user_device()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var deviceId = Guid.NewGuid();
        var manufacturer = "Samsung";
        var model = "Galaxy S10+";
        var platform = "Android";
        var version = "12.0";
        var idiom = "Phone";
        var deviceType = "Physical";
        var pnsHandler = Guid.NewGuid().ToString();
        var app = "Peers";
        var appVersion = "1.0.0";
        var user = Test2FUser(registerDevice: false).Generate();

        // Act
        var device = user.RegisterDevice(date, deviceId, manufacturer, model, platform, version, idiom, deviceType, pnsHandler,
            app, appVersion);

        // Assert
        Assert.Same(device, Assert.Single(user.DeviceList));
        Assert.Equal(deviceId, device.DeviceId);
        Assert.Equal(manufacturer, device.Manufacturer);
        Assert.Equal(model, device.Model);
        Assert.Equal(platform, device.Platform);
        Assert.Equal(version, device.OSVersion);
        Assert.Equal(idiom, device.Idiom);
        Assert.Equal(deviceType, device.DeviceType);
        Assert.Equal(pnsHandler, device.PnsHandle);
        Assert.Equal(app, device.App);
        Assert.Equal(appVersion, device.AppVersion);
        Assert.Equal(date, device.PnsHandleTimestamp);
        Assert.Equal(date, device.RegisteredOn);
    }

    [Fact]
    public void LinkDevice_links_device()
    {
        // Arrange
        var user = Test2FUser(registerDevice: false).Generate();
        user.DeviceList = [];
        var device = TestDevice(null).Generate();

        // Act
        user.LinkDevice(device);

        // Assert
        var userDevice = Assert.Single(user.DeviceList);
        Assert.Same(device, userDevice);
        Assert.Same(user, userDevice.User);
    }

    [Fact]
    public void LinkDevice_checks_business_rules()
    {
        // Arrange
        var user = Test2FUser(registerDevice: false).Generate();
        user.DeviceList = [];
        var device = TestDevice(null).Generate();

        // Act
        user.CheckedRules.Clear();
        user.LinkDevice(device);

        // Assert
        Assert.IsType<CanLinkDeviceRule>(Assert.Single(user.CheckedRules));
    }

    [Fact]
    public void UnlinkDevice_unlinks_device()
    {
        // Arrange
        var user = Test2FUser(registerDevice: false).Generate();
        var device = user.RegisterDevice(DateTime.UtcNow, Guid.NewGuid(), "x", "x", "x", "x", "x", "x", "x", "x", "x");

        // Act
        user.UnlinkDevice(device);

        // Assert
        Assert.Empty(user.DeviceList);
        Assert.Null(device.User);
    }

    [Fact]
    public void UnlinkDevice_checks_business_rules()
    {
        // Arrange
        var user = Test2FUser(registerDevice: false).Generate();
        var device = user.RegisterDevice(DateTime.UtcNow, Guid.NewGuid(), "x", "x", "x", "x", "x", "x", "x", "x", "x");

        // Act
        user.CheckedRules.Clear();
        user.UnlinkDevice(device);

        // Assert
        Assert.IsType<CanUnlinkDeviceRule>(Assert.Single(user.CheckedRules));
    }

    [Fact]
    public void GetActiveRefreshToken_returns_active_refreshToken()
    {
        // Arrange
        var user = Test2FUser(addToken: false).Generate();
        var activeRefreshToken = new RefreshToken();
        user.RefreshTokens.Add(activeRefreshToken);

        // Act
        var rt = user.GetActiveRefreshToken();

        // Assert
        Assert.Same(activeRefreshToken, rt);
    }

    [Fact]
    public void GetActiveRefreshToken_throws_when_no_active_refreshToken_exist()
    {
        // Arrange
        var user = Test2FUser(addToken: false).Generate();

        // Act and assert
        var ex = Assert.Throws<InvalidOperationException>(user.GetActiveRefreshToken);
        Assert.Equal("Sequence contains no matching element", ex.Message);
    }

    [Fact]
    public void GetActiveRefreshToken_throws_when_multiple_active_refreshTokens_exist()
    {
        /*
         * Typically, this should never happen as concurrency checks should prevent such cases. This test is being added to ensure if this happens somehow
         * we get an exception.
         *
         * */

        // Arrange
        var user = Test2FUser(addToken: false).Generate();
        user.RefreshTokens.Add(new RefreshToken());
        user.RefreshTokens.Add(new RefreshToken());

        // Act and assert
        var ex = Assert.Throws<InvalidOperationException>(user.GetActiveRefreshToken);
        Assert.Equal("Sequence contains more than one matching element", ex.Message);
    }

    [Fact]
    public void GetOrCreateRefreshToken_returns_active_refreshToken_when_found()
    {
        // Arrange
        var user = Test2FUser(addToken: false).Generate();
        var activeRefreshToken = new RefreshToken();
        user.RefreshTokens.Add(activeRefreshToken);

        // Act
        Assert.Single(user.RefreshTokens);
        var rt = user.GetOrCreateRefreshToken(DateTime.UtcNow);

        // Assert
        Assert.Same(activeRefreshToken, rt);
        Assert.Same(activeRefreshToken, Assert.Single(user.RefreshTokens));
    }

    [Fact]
    public void GetOrCreateRefreshToken_creates_new_refreshToken_when_none_exist()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var user = Test2FUser(addToken: false).Generate();

        // Act
        Assert.Empty(user.RefreshTokens);
        var rt = user.GetOrCreateRefreshToken(date);

        // Assert
        Assert.Same(rt, Assert.Single(user.RefreshTokens));
        Assert.Equal(date, rt.Created);
    }

    [Fact]
    public void GetOrCreateRefreshToken_creates_new_refreshToken_when_no_active_tokens_exist()
    {
        // Arrange
        var user = Test2FUser(addToken: false).Generate();
        var revokedRefreshToken = new RefreshToken() { Revoked = DateTime.UtcNow };
        user.RefreshTokens.Add(revokedRefreshToken);

        // Act
        Assert.Single(user.RefreshTokens);
        var rt = user.GetOrCreateRefreshToken(DateTime.UtcNow);

        // Assert
        Assert.NotSame(revokedRefreshToken, rt);
        Assert.Equal(2, user.RefreshTokens.Count);
    }

    [Fact]
    public void TryRefreshToken_fails_when_user_is_banned()
    {
        // Arrange
        var user = Test2FUser(addToken: false).Generate();
        var rt = user.GetOrCreateRefreshToken(DateTime.UtcNow);
        user.ChangeStatus(DateTime.UtcNow, TestPwUser().Generate(), UserStatus.Banned, "test");

        // Act
        var result = user.TryRefreshToken(DateTime.UtcNow, rt.Token, out var newToken);

        // Assert
        Assert.False(result);
        Assert.Null(newToken);
        Assert.Single(user.RefreshTokens);
    }

    [Fact]
    public void TryRefreshToken_fails_when_user_account_is_deleted()
    {
        // Arrange
        var user = Test2FUser(addToken: false).Generate();
        var rt = user.GetOrCreateRefreshToken(DateTime.UtcNow);
        user.DeleteAccount(DateTime.UtcNow);

        // Act
        var result = user.TryRefreshToken(DateTime.UtcNow, rt.Token, out var newToken);

        // Assert
        Assert.False(result);
        Assert.Null(newToken);
        Assert.Single(user.RefreshTokens);
    }

    [Fact]
    public void TryRefreshToken_fails_when_token_is_revoked()
    {
        // Arrange
        var user = Test2FUser(addToken: false).Generate();
        var expiredRefreshToken = new RefreshToken { Token = "token", Revoked = DateTime.UtcNow };
        user.RefreshTokens.Add(expiredRefreshToken);

        // Act
        var result = user.TryRefreshToken(DateTime.UtcNow, expiredRefreshToken.Token, out var newToken);

        // Assert
        Assert.False(result);
        Assert.Null(newToken);
        Assert.Single(user.RefreshTokens);
    }

    [Fact]
    public void TryRefreshToken_fails_when_token_doesnt_exist()
    {
        // Arrange
        var user = Test2FUser(addToken: false).Generate();

        // Act
        var result = user.TryRefreshToken(DateTime.UtcNow, "none existing", out var newToken);

        // Assert
        Assert.False(result);
        Assert.Null(newToken);
        Assert.Empty(user.RefreshTokens);
    }

    [Fact]
    public void TryRefreshToken_acquires_new_token_and_revokes_current_when_current_is_valid_and_user_not_banned()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var user = Test2FUser(addToken: false).Generate();
        var activeRefreshToken = new RefreshToken { Token = "token" };
        user.RefreshTokens.Add(activeRefreshToken);

        // Act
        var result = user.TryRefreshToken(date, activeRefreshToken.Token, out var newToken);

        // Assert
        Assert.True(result);
        Assert.NotNull(newToken);
        Assert.Equal(2, user.RefreshTokens.Count);
        Assert.NotNull(activeRefreshToken.Revoked);
        Assert.Equal(date, activeRefreshToken.Revoked.Value);
    }

    [Fact]
    public void UpdatePnsHandle_throws_on_empty_deviceId()
    {
        // Arrange
        var user = Test2FUser().Generate();

        // Act & assert
        var ex = Assert.Throws<ArgumentException>(() => user.UpdatePnsHandle(DateTime.UtcNow, Guid.Empty, Guid.NewGuid().ToString(), out var oldHandle));
        Assert.Equal("Device ID cannot be empty. (Parameter 'deviceId')", ex.Message);
    }

    [Fact]
    public void UpdatePnsHandle_returns_false_when_device_not_found()
    {
        // Arrange
        var user = Test2FUser().Generate();

        // Act
        var result = user.UpdatePnsHandle(DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid().ToString(), out var oldHandle);

        // Assert
        Assert.False(result);
        Assert.Null(oldHandle);
    }

    [Fact]
    public void UpdatePnsHandle_updates_device_handle_and_returns_old_handle()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var handle = Guid.NewGuid().ToString();
        var user = Test2FUser().Generate();
        var prevHandle = user.DeviceList.First().PnsHandle;

        // Act
        var result = user.UpdatePnsHandle(date, user.DeviceList.First().DeviceId, handle, out var oldHandle);

        // Assert
        Assert.True(result);
        Assert.Equal(handle, user.DeviceList.First().PnsHandle);
        Assert.Equal(prevHandle, oldHandle);
        Assert.Equal(date, user.DeviceList.First().PnsHandleLastRefreshed);
        Assert.Equal(date, user.DeviceList.First().PnsHandleTimestamp);
    }

    [Fact]
    public void UpdatePnsHandle_can_remove_device_handle()
    {
        // Arrange
        var user = Test2FUser().Generate();
        var prevHandle = user.DeviceList.First().PnsHandle;

        // Act
        var result = user.UpdatePnsHandle(DateTime.UtcNow, user.DeviceList.First().DeviceId, null, out var oldHandle);

        // Assert
        Assert.True(result);
        Assert.Null(user.DeviceList.First().PnsHandle);
        Assert.Equal(prevHandle, oldHandle);
    }

    [Fact]
    public void ChangeStatus_appends_status_change_entry()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var manager = TestPwUser().Generate();
        var newStatus = UserStatus.Suspended;
        var reason = "test reason";
        var user = Test2FUser().Generate();

        // Act
        user.ChangeStatus(date, manager, newStatus, reason);

        // Assert
        var statusChangeEntry = Assert.Single(user.StatusChangeHistory);
        Assert.Equal(date, statusChangeEntry.ChangedOn);
        Assert.Same(manager, statusChangeEntry.ChangedBy);
        Assert.Equal(UserStatus.Active, statusChangeEntry.OldStatus);
        Assert.Equal(UserStatus.Suspended, statusChangeEntry.NewStatus);
        Assert.Equal(reason, statusChangeEntry.ChangeReason);
    }

    [Fact]
    public void ChangeStatus_sets_newStatus()
    {
        // Arrange
        var newStatus = UserStatus.Suspended;
        var user = Test2FUser().Generate();

        // Act
        user.ChangeStatus(DateTime.UtcNow, TestPwUser().Generate(), newStatus, "test reason");

        // Assert
        Assert.Equal(UserStatus.Suspended, user.Status);
    }

    [Fact]
    public void ChangeStatus_checks_business_rules()
    {
        // Arrange
        var user = Test2FUser().Generate();

        // Act
        user.CheckedRules.Clear();
        user.ChangeStatus(DateTime.UtcNow, TestPwUser().Generate(), UserStatus.Suspended, "test reason");

        // Assert
        Assert.IsType<CanChangeUserStatusRule>(Assert.Single(user.CheckedRules));
    }

    [Theory]
    [InlineData(UserStatus.Banned)]
    [InlineData(UserStatus.Deleted)]
    public void ChangeStatus_revokes_active_tokens_when_changing_to_banned_or_deleted(UserStatus newStatus)
    {
        // Arrange
        var user = Test2FUser().Generate();
        var activeRefreshToken = user.GetOrCreateRefreshToken(DateTime.UtcNow);
        var date = DateTime.UtcNow;

        // Act
        user.ChangeStatus(date, TestPwUser().Generate(), newStatus, "test");

        // Assert
        Assert.NotNull(activeRefreshToken.Revoked);
        Assert.Equal(date, activeRefreshToken.Revoked.Value);
    }

    [Fact]
    public void AddNotification_adds_notification()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var user = Test2FUser().Generate();
        var notification = Notification.Create(date, "test");

        // Act
        user.AddNotification(notification);

        // Assert
        var userNotification = Assert.Single(user.Notifications);
        Assert.Same(user, userNotification.User);
        Assert.Same(notification, userNotification.Notification);
        Assert.Equal(date, notification.CreatedOn);

    }

    [Fact]
    public void DeleteAccount_flags_account_as_deleted()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var user = Test2FUser().Generate();

        // Act
        user.DeleteAccount(date);

        // Assert
        Assert.True(user.IsDeleted);
        Assert.NotNull(user.DeletedOn);
        Assert.Equal(date, user.DeletedOn.Value);
        Assert.Equal(UserStatus.Deleted, user.Status);
    }

    [Fact]
    public void DeleteAccount_appends_deleted_to_username_email_and_phoneNumber()
    {
        // Arrange
        var user = Test2FUser().Generate();
        user.NormalizedUserName = user.UserName.ToUpperInvariant();
        user.Email = "test@domain.com";
        user.NormalizedEmail = user.Email.ToUpperInvariant();

        // Act
        user.DeleteAccount(DateTime.UtcNow);

        // Assert
        Assert.EndsWith("_deleted", user.UserName);
        Assert.EndsWith("_deleted", user.NormalizedUserName);
        Assert.EndsWith("_deleted", user.Email);
        Assert.EndsWith("_deleted", user.NormalizedEmail);
        Assert.EndsWith("_deleted", user.PhoneNumber);
    }

    [Fact]
    public void DeleteAccount_does_not_append_anything_to_phoneNumber_if_already_null()
    {
        // Arrange
        var user = Test2FUser().Generate();
        user.PhoneNumber = null;

        // Act
        user.DeleteAccount(DateTime.UtcNow);

        // Assert
        Assert.Null(user.PhoneNumber);
    }

    [Fact]
    public void DeleteAccount_copies_original_username()
    {
        // Arrange
        var user = Test2FUser().Generate();
        var username = user.UserName;

        // Act
        user.DeleteAccount(DateTime.UtcNow);

        // Assert
        Assert.Equal(username, user.OriginalDeletedUsername);
    }

    [Fact]
    public void DeleteAccount_unlinks_all_linked_devices()
    {
        // Arrange
        var user = Test2FUser().Generate();
        var device1 = TestDevice(user).Generate();
        var device2 = TestDevice(user).Generate();
        user.LinkDevice(device1);
        user.LinkDevice(device2);

        // Act
        user.DeleteAccount(DateTime.UtcNow);

        // Assert
        Assert.Empty(user.DeviceList);
        Assert.Null(device1.User);
        Assert.Null(device2.User);
    }

    [Fact]
    public void DeleteAccount_noops_when_account_is_already_deleted()
    {
        // Arrange
        var user = Test2FUser().Generate();
        user.DeleteAccount(DateTime.UtcNow);

        // Act
        user.DeleteAccount(DateTime.UtcNow);

        // Assert
        // No exception
    }

    [Theory]
    [InlineData(" John ", null, null, null)]
    [InlineData(null, " Doe ", null, null)]
    [InlineData(null, null, " example@domain.com ", null)]
    [InlineData(null, null, null, " ar ")]
    public void SetProfile_updates_profile_based_on_non_null_values(string firstname, string lastname, string email, string preferredLanguage)
    {
        // Arrange
        var user = Test2FUser().Generate();

        // Act
        user.SetProfile(firstname, lastname, email, preferredLanguage);

        // Assert
        Assert.Equal(firstname?.Trim() ?? user.Firstname, user.Firstname);
        Assert.Equal(lastname?.Trim() ?? user.Lastname, user.Lastname);
        Assert.Equal(email?.Trim(), user.UpdatedEmail);
        Assert.Equal(preferredLanguage?.Trim() ?? user.PreferredLanguage, user.PreferredLanguage);
    }

    [Fact]
    public void SetProfile_throws_when_invalid_preferredLang_length()
    {
        // Arrange, act and assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Test2FUser().Generate().SetProfile(null, null, null, "too long"));
        Assert.Equal("Preferred language must be a two-letter ISO code. (Parameter 'preferredLanguage')", ex.Message);
    }

    [Fact]
    public void InitiateEmailUpdate_throws_on_invalid_emailAddress()
    {
        // Arrange
        var user = Test2FUser().Generate();

        // Act & assert
        var ex = Assert.Throws<ArgumentException>(() => user.InitiateEmailUpdate("invalid"));
        Assert.Equal("Invalid email address format. (Parameter 'email')", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("current@example.com")]
    public void InitiateEmailUpdate_does_nothing_when_newEmail_is_null(string currentEmail)
    {
        // Arrange
        var user = Test2FUser().Generate();
        user.Email = currentEmail;

        // Act
        user.InitiateEmailUpdate(null);

        // Assert
        Assert.Equal(currentEmail, user.Email);
        Assert.Null(user.UpdatedEmail);
    }

    [Theory]
    [InlineData(null, false, "new@example.com")]
    [InlineData("current@example.com", true, " new@example.com ")]
    [InlineData("current@example.com", false, " new@example.com ")]
    public void InitiateEmailUpdate_sets_updatedEmail_and_resets_emailConfirmed_flag_when_different_than_current_email(string currentEmail, bool emailConfirmed, string newEmail)
    {
        // Arrange
        var user = Test2FUser().Generate();
        user.Email = currentEmail;
        user.EmailConfirmed = emailConfirmed;

        // Act
        user.InitiateEmailUpdate(newEmail);

        // Assert
        Assert.Equal(currentEmail, user.Email);
        Assert.Equal(newEmail.Trim(), user.UpdatedEmail);
        Assert.False(user.EmailConfirmed);
    }

    [Fact]
    public void InitiateEmailUpdate_sets_updatedEmail_when_already_set()
    {
        // Arrange
        var user = Test2FUser().Generate();
        user.UpdatedEmail = "update1@example.com";

        // Act
        user.InitiateEmailUpdate(" update2@example.com ");

        // Assert
        Assert.Equal("update2@example.com", user.UpdatedEmail);
    }

    [Theory]
    [InlineData(" current@example.com ", true)]
    [InlineData(" current@example.com ", false)]
    public void InitiateEmailUpdate_does_nothing_when_newEmail_is_same_as_current_email(string currentEmail, bool emailConfirmed)
    {
        // Arrange
        var user = Test2FUser().Generate();
        user.Email = currentEmail.Trim();
        user.EmailConfirmed = emailConfirmed;

        // Act
        user.InitiateEmailUpdate(currentEmail);

        // Assert
        Assert.Equal(currentEmail.Trim(), user.Email);
        Assert.Null(user.UpdatedEmail);
        Assert.Equal(emailConfirmed, user.EmailConfirmed);
    }

    [Fact]
    public void RecordAppUsed_records_usage_entry()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var user = Test2FUser().Generate();

        // Act
        user.RecordAppUsed(date);

        // Assert
        var usage = Assert.Single(user.AppUsage);
        Assert.Equal(date, usage.OpenedAt);
    }
}
