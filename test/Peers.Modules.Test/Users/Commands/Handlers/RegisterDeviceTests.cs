using Peers.Core.Commands;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Users;
using Peers.Modules.Users.Commands;
using Peers.Modules.Users.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Peers.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class RegisterDeviceTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_customer_or_driver_or_partner_role()
        => await AssertCommandAccess(TestRegisterDevice.Generate(), [Roles.Customer]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Registers_device()
    {
        // Arrange
        var customer = await EnrollCustomer();
        var cmd = TestRegisterDevice.Generate();

        // Act
        var result = await SendAsync(cmd, customer);

        // Assert
        var id = Assert.IsType<Created<IdObj>>(result).ExtractId();
        customer = await FindAsync<Customer>(customer.Id, "User.DeviceList");
        var device = Assert.Single(customer.User.DeviceList, p => p.Id == id);
        Assert.Equal(cmd.Id, device.DeviceId);
        Assert.Equal(cmd.Manufacturer, device.Manufacturer);
        Assert.Equal(cmd.Model, device.Model);
        Assert.Equal(cmd.Platform, device.Platform);
        Assert.Equal(cmd.OSVersion, device.OSVersion);
        Assert.Equal(cmd.Idiom, device.Idiom);
        Assert.Equal(cmd.Type, device.DeviceType);
        Assert.Equal(cmd.PnsHandle, device.PnsHandle);
        Assert.Equal(cmd.App, device.App);
        Assert.Equal(cmd.AppVersion, device.AppVersion);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Subscribes_user_to_appropriate_topic_upon_device_registration()
    {
        // Arrange
        var sysUser = await EnrollCustomer();
        var user = await FindAsync<AppUser>(sysUser.Id, "DeviceList");
        var cmd = TestRegisterDevice.Generate();
        FirebaseMoq.Setup(p => p.SubscribeToTopicAsync(cmd.PnsHandle, $"{FirebaseMessagingServiceExtensions.CustomersTopic}-en")).Verifiable();

        // Act
        await SendAsync(cmd, user);

        // Assert
        FirebaseMoq.Verify();
        FirebaseMoq.Reset();
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_badRequest_when_device_exist_and_a_trackingId_is_set()
    {
        // Arrange
        var customer = await EnrollCustomer();

        // Act
        var result = await SendAsync(TestRegisterDevice.Generate() with { Id = customer.User.DeviceList.Single().DeviceId, TrackingId = "123" }, customer);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Invalid device registration tracking request. A tracking request was not expected.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Noops_and_returns_noContent_when_registering_same_deviceId_multiple_times_with_same_appVersion_and_pnsHandler()
    {
        // Arrange
        var customer = await EnrollCustomer();
        var cmd = TestRegisterDevice.Generate();
        Assert.IsType<Created<IdObj>>(await SendAsync(cmd, customer)).ExtractId();

        // Act
        var result = await SendAsync(cmd, customer);

        // Assert
        Assert.IsType<NoContent>(result);
        customer = await FindAsync<Customer>(customer.Id, "User.DeviceList");
        Assert.Single(customer.User.DeviceList, p => p.DeviceId == cmd.Id);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Updates_appVersion_and_pnsHandler_and_returns_noContent_when_registering_save_deviceId_multiple_times()
    {
        // Arrange
        var customer = await EnrollCustomer();
        var cmd = TestRegisterDevice.Generate();
        Assert.IsType<Created<IdObj>>(await SendAsync(cmd, customer)).ExtractId();

        // Act
        cmd = cmd with { AppVersion = "3.5", PnsHandle = "123" };
        var result = await SendAsync(cmd, customer);

        // Assert
        Assert.IsType<NoContent>(result);
        customer = await FindAsync<Customer>(customer.Id, "User.DeviceList");
        var device = Assert.Single(customer.User.DeviceList, p => p.DeviceId == cmd.Id);
        Assert.Equal("3.5", device.AppVersion);
        Assert.Equal("123", device.PnsHandle);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requests_deviceId_and_pnsHandle_refresh_when_registering_a_device_owned_by_another_user()
    {
        // Arrange
        var customer1 = await EnrollCustomer();
        var customer2 = await EnrollCustomer();
        var cmd = TestRegisterDevice.Generate();

        var deviceId = Assert.IsType<Created<IdObj>>(await SendAsync(cmd, customer1)).ExtractId();

        // Act
        var result = await SendAsync(cmd, customer2);

        // Assert
        var acceptedResult = Assert.IsType<Accepted<RegisterDevice.Response>>(result);
        Assert.Equal(8, acceptedResult.Value.TrackingId.Length);

        customer1 = await FindAsync<Customer>(customer1.Id, "User.DeviceList");
        customer2 = await FindAsync<Customer>(customer2.Id, "User.DeviceList");

        Assert.Single(customer1.User.DeviceList, p => p.DeviceId == cmd.Id);
        Assert.DoesNotContain(customer2.User.DeviceList, p => p.DeviceId == cmd.Id);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_badRequest_when_registering_a_new_device_with_a_non_existing_trackingId()
    {
        // Arrange
        var customer = await EnrollCustomer();
        var cmd = TestRegisterDevice.Generate() with { TrackingId = "123" };

        // Act
        var result = await SendAsync(cmd, customer);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Invalid device registration tracking request. Tracking id not found.", problem.Detail);

        customer = await FindAsync<Customer>(customer.Id, "User.DeviceList");
        Assert.DoesNotContain(customer.User.DeviceList, p => p.DeviceId == cmd.Id);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_badRequest_when_registering_a_new_device_and_a_trackingId_contains_an_invalid_oldDeviceId()
    {
        // Arrange
        var customer1 = await EnrollCustomer();
        var customer2 = await EnrollCustomer();
        var cmd = TestRegisterDevice.Generate();

        var originalDeviceId = Assert.IsType<Created<IdObj>>(await SendAsync(cmd, customer1)).ExtractId();
        var accepted = Assert.IsType<Accepted<RegisterDevice.Response>>(await SendAsync(cmd, customer2));

        await ExecuteDbContextAsync(async db => await db.Set<Device>().Where(p => p.Id == originalDeviceId).ExecuteDeleteAsync());

        // Act
        var newCmd = cmd with { Id = Guid.NewGuid(), PnsHandle = Guid.NewGuid().ToString(), TrackingId = accepted.Value.TrackingId };
        var result = await SendAsync(newCmd, customer2);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Invalid device registration tracking request. Device not found.", problem.Detail);

        customer1 = await FindAsync<Customer>(customer1.Id, "User.DeviceList");
        customer2 = await FindAsync<Customer>(customer2.Id, "User.DeviceList");
        Assert.DoesNotContain(customer1.User.DeviceList, p => p.DeviceId == cmd.Id); // It was manually deleted
        Assert.DoesNotContain(customer2.User.DeviceList, p => p.DeviceId == newCmd.Id);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_badRequest_when_registering_a_new_device_with_trackingId_but_same_pnsHandle()
    {
        // Arrange
        var customer1 = await EnrollCustomer();
        var customer2 = await EnrollCustomer();
        var cmd = TestRegisterDevice.Generate();

        var originalDeviceId = Assert.IsType<Created<IdObj>>(await SendAsync(cmd, customer1)).ExtractId();
        var accepted = Assert.IsType<Accepted<RegisterDevice.Response>>(await SendAsync(cmd, customer2));

        // Act
        var newCmd = cmd with { Id = Guid.NewGuid(), TrackingId = accepted.Value.TrackingId };
        var result = await SendAsync(newCmd, customer2);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Invalid device registration tracking request. PNS handle needs update.", problem.Detail);

        customer1 = await FindAsync<Customer>(customer1.Id, "User.DeviceList");
        customer2 = await FindAsync<Customer>(customer2.Id, "User.DeviceList");
        Assert.DoesNotContain(customer2.User.DeviceList, p => p.DeviceId == newCmd.Id);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Registers_a_device_previously_owned_by_another_user_and_removes_the_old_one()
    {
        // Arrange
        var customer1 = await EnrollCustomer();
        var customer2 = await EnrollCustomer();
        var cmd = TestRegisterDevice.Generate();

        var originalDeviceId = Assert.IsType<Created<IdObj>>(await SendAsync(cmd, customer1)).ExtractId();
        var accepted = Assert.IsType<Accepted<RegisterDevice.Response>>(await SendAsync(cmd, customer2));

        // Act
        var newCmd = cmd with { Id = Guid.NewGuid(), PnsHandle = Guid.NewGuid().ToString(), TrackingId = accepted.Value.TrackingId };
        FirebaseMoq.Setup(p => p.UnsubscribeFromTopicAsync(cmd.PnsHandle, "customers-en")).Verifiable();
        var result = await SendAsync(newCmd, customer2);

        // Assert
        var id = Assert.IsType<Created<IdObj>>(result).ExtractId();
        customer1 = await FindAsync<Customer>(customer1.Id, "User.DeviceList");
        customer2 = await FindAsync<Customer>(customer2.Id, "User.DeviceList");

        var device = Assert.Single(customer2.User.DeviceList, p => p.Id == id);
        Assert.Equal(newCmd.Id, device.DeviceId);
        Assert.Equal(newCmd.Manufacturer, device.Manufacturer);
        Assert.Equal(newCmd.Model, device.Model);
        Assert.Equal(newCmd.Platform, device.Platform);
        Assert.Equal(newCmd.OSVersion, device.OSVersion);
        Assert.Equal(newCmd.Idiom, device.Idiom);
        Assert.Equal(newCmd.Type, device.DeviceType);
        Assert.Equal(newCmd.PnsHandle, device.PnsHandle);

        customer1 = await FindAsync<Customer>(customer1.Id, "User.DeviceList");
        Assert.Null(customer1.User.DeviceList.SingleOrDefault(p => p.DeviceId == cmd.Id));

        FirebaseMoq.Verify();
        FirebaseMoq.Reset();
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Subscribes_user_to_appropriate_topic_upon_device_registration_ownership_switch()
    {
        // Arrange
        var customer1 = await EnrollCustomer();
        var customer2 = await EnrollCustomer();
        var cmd = TestRegisterDevice.Generate();

        var originalDeviceId = Assert.IsType<Created<IdObj>>(await SendAsync(cmd, customer1)).ExtractId();
        var accepted = Assert.IsType<Accepted<RegisterDevice.Response>>(await SendAsync(cmd, customer2));

        // Act
        var newCmd = cmd with { Id = Guid.NewGuid(), PnsHandle = Guid.NewGuid().ToString(), TrackingId = accepted.Value.TrackingId };
        FirebaseMoq.Setup(p => p.SubscribeToTopicAsync(newCmd.PnsHandle, "customers-en")).Verifiable();
        await SendAsync(newCmd, customer2);

        // Assert
        FirebaseMoq.Verify();
        FirebaseMoq.Reset();
    }
}
