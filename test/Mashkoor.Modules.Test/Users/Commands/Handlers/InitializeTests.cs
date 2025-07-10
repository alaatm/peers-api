using Microsoft.AspNetCore.Http.HttpResults;
using Mashkoor.Modules.Users.Commands;
using Mashkoor.Modules.Users.Domain;
using Mashkoor.Modules.Users.Events;
using Mashkoor.Modules.System.Domain;

namespace Mashkoor.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class InitializeTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_customer_role()
        => await AssertCommandAccess(new Initialize.Command(default), [Roles.Customer]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Proxies_call_for_customer()
    {
        // Arrange
        InsertTestClientAppInfo();
        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();

        // Act
        var result = await SendAsync(new Initialize.Command(TestRegisterDevice), customer);

        // Assert
        var okResult = AssertX.IsType<Ok<Initialize.Response>>(result);
        var response = okResult.Value;
        Assert.NotEmpty(response.SupportedLanguages);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Publishes_AppOpened_event()
    {
        // Arrange
        InsertTestClientAppInfo();
        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();
        ProducerMoq.Reset();

        // Act
        var result = await SendAsync(new Initialize.Command(TestRegisterDevice), customer);

        // Assert
        ProducerMoq.Verify(p => p.PublishAsync(It.Is<AppOpened>(p =>
            p.UserId == customer.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Handles_initial_device_registration()
    {
        // Arrange
        InsertTestClientAppInfo();
        var registerDeviceCmd = TestRegisterDevice.Generate();
        var customer = await EnrollCustomer(registerDevice: false);

        // Act
        var result = await SendAsync(new Initialize.Command(registerDeviceCmd), customer);

        // Assert
        var okResult = AssertX.IsType<Ok<Initialize.Response>>(result);
        var response = okResult.Value;
        Assert.Equal(201, response.DeviceResponse.Status);
        Assert.Null(response.DeviceResponse.TrackingId);

        var user = await FindAsync<AppUser>(customer.Id, "DeviceList");
        var device = Assert.Single(user.DeviceList);
        Assert.Equal(registerDeviceCmd.Id, device.DeviceId);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Handles_same_device_registration()
    {
        // Arrange
        InsertTestClientAppInfo();
        var customer = await EnrollCustomer();
        var registerDeviceCmd = TestRegisterDevice.Generate() with { Id = customer.User.DeviceList.First().DeviceId, AppVersion = "99", PnsHandle = "9999" };

        // Act
        var result = await SendAsync(new Initialize.Command(registerDeviceCmd), customer);

        // Assert
        var okResult = AssertX.IsType<Ok<Initialize.Response>>(result);
        var response = okResult.Value;
        Assert.Equal(204, response.DeviceResponse.Status);
        Assert.Null(response.DeviceResponse.TrackingId);

        var user = await FindAsync<AppUser>(customer.Id, "DeviceList");
        var device = Assert.Single(user.DeviceList);
        Assert.Equal(registerDeviceCmd.Id, device.DeviceId);
        Assert.Equal("99", device.AppVersion);
        Assert.Equal("9999", device.PnsHandle);
    }

    [SkippableTheory(typeof(PlatformNotSupportedException))]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handles_device_ownership_switch(bool newUserAlreadyHasDevice)
    {
        // Arrange
        InsertTestClientAppInfo();
        var customer1 = await EnrollCustomer();
        var customer2 = await EnrollCustomer(registerDevice: newUserAlreadyHasDevice);
        var registerDeviceCmd = TestRegisterDevice.Generate() with { Id = customer1.User.DeviceList.First().DeviceId };

        // Act
        var result = await SendAsync(new Initialize.Command(registerDeviceCmd), customer2);

        // Assert
        var okResult = AssertX.IsType<Ok<Initialize.Response>>(result);
        var response = okResult.Value;
        Assert.Equal(202, response.DeviceResponse.Status);
        Assert.NotNull(response.DeviceResponse.TrackingId);

        var user2 = await FindAsync<AppUser>(customer2.Id, "DeviceList");
        if (newUserAlreadyHasDevice)
        {
            Assert.NotEqual(registerDeviceCmd.Id, Assert.Single(user2.DeviceList).DeviceId);
        }
        else
        {
            Assert.Empty(user2.DeviceList);
        }
    }

    [SkippableTheory(typeof(PlatformNotSupportedException))]
    [InlineData("Android")]
    [InlineData("iOS")]
    public async Task Sets_store_link_to_null_when_malformed_client_version(string platform)
    {
        // Arrange
        var appInfo = InsertTestClientAppInfo();
        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();

        var cmd = TestRegisterDevice.Generate() with { AppVersion = "malformed", Platform = platform, App = "com.example.app" };

        // Act
        var result = await SendAsync(new Initialize.Command(cmd), customer);

        // Assert
        var okResult = AssertX.IsType<Ok<Initialize.Response>>(result);
        var response = okResult.Value;
        Assert.Null(response.UpdateLink);
        Assert.Equal(appInfo.LatestVersion.VersionString, response.LatestVersion);
        ClearIdentity();
    }

    [SkippableTheory(typeof(PlatformNotSupportedException))]
    [InlineData("Android")]
    [InlineData("iOS")]
    public async Task Sets_store_link_to_null_when_app_version_is_up_to_date(string platform)
    {
        // Arrange
        var appInfo = InsertTestClientAppInfo();
        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();

        var cmd = TestRegisterDevice.Generate() with { AppVersion = "1.0.0 (0)", Platform = platform, App = "com.example.app" };

        // Act
        var result = await SendAsync(new Initialize.Command(cmd), customer);

        // Assert
        var okResult = AssertX.IsType<Ok<Initialize.Response>>(result);
        var response = okResult.Value;
        Assert.Null(response.UpdateLink);
        Assert.Equal(appInfo.LatestVersion.VersionString, response.LatestVersion);
        ClearIdentity();
    }

    [SkippableTheory(typeof(PlatformNotSupportedException))]
    [InlineData("Android")]
    [InlineData("iOS")]
    public async Task Sets_store_link_to_null_when_app_version_is_greater_than_latest(string platform)
    {
        // Arrange
        var appInfo = InsertTestClientAppInfo();
        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();

        var cmd = TestRegisterDevice.Generate() with { AppVersion = "2.0.0 (0)", Platform = platform, App = "com.example.app" };

        // Act
        var result = await SendAsync(new Initialize.Command(cmd), customer);

        // Assert
        var okResult = AssertX.IsType<Ok<Initialize.Response>>(result);
        var response = okResult.Value;
        Assert.Null(response.UpdateLink);
        Assert.Equal(appInfo.LatestVersion.VersionString, response.LatestVersion);
        ClearIdentity();
    }

    [SkippableTheory(typeof(PlatformNotSupportedException))]
    [InlineData("Android", "android_link")]
    [InlineData("iOS", "ios_link")]
    public async Task Sets_store_link_when_version_is_less_that_latest(string platform, string expectedStoreLink)
    {
        // Arrange
        var appInfo = InsertTestClientAppInfo(major: 2);
        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();

        var cmd = TestRegisterDevice.Generate() with { AppVersion = "1.0.0 (0)", Platform = platform, App = "com.example.app" };

        // Act
        var result = await SendAsync(new Initialize.Command(cmd), customer);

        // Assert
        var okResult = AssertX.IsType<Ok<Initialize.Response>>(result);
        var response = okResult.Value;
        Assert.Equal(expectedStoreLink, response.UpdateLink);
        Assert.Equal(appInfo.LatestVersion.VersionString, response.LatestVersion);
        ClearIdentity();
    }

    [SkippableTheory(typeof(PlatformNotSupportedException))]
    [InlineData("Android")]
    [InlineData("iOS")]
    public async Task Sets_store_link_and_latestVersion_to_null_when_client_app_entry_does_not_exist(string platform)
    {
        // Arrange
        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();

        var cmd = TestRegisterDevice.Generate() with { AppVersion = "1.0.0 (0)", Platform = platform, App = "com.example.app" };

        // Act
        var result = await SendAsync(new Initialize.Command(cmd), customer);

        // Assert
        var okResult = AssertX.IsType<Ok<Initialize.Response>>(result);
        var response = okResult.Value;
        Assert.Null(response.UpdateLink);
        Assert.Null(response.LatestVersion);
        ClearIdentity();
    }

    private ClientAppInfo InsertTestClientAppInfo(int major = 1, int minor = 0, int build = 0, int revision = 0)
    {
        var clientApp = new ClientAppInfo()
        {
            PackageName = "com.example.app",
            HashString = "test",
            LatestVersion = new ClientAppVersion
            {
                Major = major,
                Minor = minor,
                Build = build,
                Revision = revision,
            },
            AndroidStoreLink = "android_link",
            IOSStoreLink = "ios_link",
        };

        ExecuteDbContext(db =>
        {
            db.ClientApps.Add(clientApp);
            db.SaveChanges();
        });

        return clientApp;
    }
}
