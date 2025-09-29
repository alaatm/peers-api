using Peers.Core.Commands;
using Peers.Core.Http;
using Peers.Modules.SystemInfo.Domain;
using Peers.Modules.Users.Commands;
using Peers.Modules.Users.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Peers.Modules.Test.Users.Commands.Handlers;

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
        Assert.NotNull(response.DeviceResponse);
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

public class NonIntegrationInitializeTests
{

    [Theory]
    [MemberData(nameof(FromRegisterDeviceResponseResult_returns_expected_response_TestData))]
    public void FromRegisterDeviceResponseResult_returns_expected_response(IResult result, int expectedStatus, string expectedTrackingId)
    {
        // Arrange & act
        var response = Initialize.FromRegisterDeviceResponseResult(result);

        // Assert
        Assert.Equal(expectedStatus, response.Status);
        Assert.Equal(expectedTrackingId, response.TrackingId);
    }

    public static TheoryData<IResult, int, string> FromRegisterDeviceResponseResult_returns_expected_response_TestData => new()
        {
            { Result.Created<IdObj>(), 201, null },
            { Result.NoContent(), 204, null },
            { Result.Accepted(null, new RegisterDevice.Response("tracking-id")), 202, "tracking-id" },
            { Result.BadRequest(), 400, null },
            { Result.Unauthorized(), 401, null },
            { new NullStatusCodeResult(), 400, null },
            { null, 500, null },
        };

    private class NullStatusCodeResult : IResult, IStatusCodeHttpResult
    {
        int? IStatusCodeHttpResult.StatusCode => null;

        public Task ExecuteAsync(HttpContext httpContext) => throw new NotImplementedException();
    }
}
