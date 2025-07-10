using Microsoft.AspNetCore.Http.HttpResults;
using Mashkoor.Modules.Users.Commands;
using Mashkoor.Modules.Users.Domain;
using Mashkoor.Modules.Users;

namespace Mashkoor.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class UpdatePnsHandleTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_customer_or_driver_or_partner_role()
        => await AssertCommandAccess(TestUpdatePnsHandle, [Roles.Customer]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_not_found_when_device_not_found()
    {
        // Arrange
        var customer = await EnrollCustomer();

        // Act
        var result = await SendAsync(TestUpdatePnsHandle, customer);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("The specified device does not exist.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Updates_device_handle()
    {
        // Arrange
        var customer = await EnrollCustomer();
        var cmd = TestUpdatePnsHandle with { DeviceId = customer.User.DeviceList.First().DeviceId };

        // Act
        var result = await SendAsync(cmd, customer);

        // Assert
        Assert.IsType<NoContent>(result);
        var device = (await FindAsync<AppUser>(p => p.UserName == customer.Username, "DeviceList")).DeviceList.First();
        Assert.Equal(cmd.PnsHandle, device.PnsHandle);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Subscribes_user_to_appropriate_topic()
    {
        // Arrange
        var sysUser = await EnrollCustomer();
        var user = await FindAsync<AppUser>(sysUser.Id, "DeviceList");
        var cmd = TestUpdatePnsHandle with { DeviceId = user.DeviceList.First().DeviceId };
        FirebaseMoq.Setup(p => p.SubscribeToTopicAsync(cmd.PnsHandle, $"{FirebaseMessagingServiceExtensions.CustomersTopic}-en")).Verifiable();

        // Act
        var result = await SendAsync(cmd, user);

        // Assert
        FirebaseMoq.Verify();
        FirebaseMoq.Reset();
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Unsubscribes_old_handle_from_appropriate_user_topic()
    {
        // Arrange
        var sysUser = await EnrollCustomer();
        var user = await FindAsync<AppUser>(sysUser.Id, "DeviceList");
        var cmd = TestUpdatePnsHandle with { DeviceId = user.DeviceList.First().DeviceId };
        Assert.IsType<NoContent>(await SendAsync(cmd, user));
        var oldHandle = cmd.PnsHandle;
        FirebaseMoq.Setup(p => p.UnsubscribeFromTopicAsync(oldHandle, $"{FirebaseMessagingServiceExtensions.CustomersTopic}-en")).Verifiable();
        cmd = TestUpdatePnsHandle with { DeviceId = user.DeviceList.First().DeviceId };

        // Act
        var result = await SendAsync(cmd, user);

        // Assert
        FirebaseMoq.Verify();
        FirebaseMoq.Reset();
    }

    private static UpdatePnsHandle.Command TestUpdatePnsHandle => new(Guid.NewGuid(), Guid.NewGuid().ToString());
}
