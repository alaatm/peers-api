using Microsoft.AspNetCore.Http.HttpResults;
using Mashkoor.Core.Commands;
using Mashkoor.Core.Queries;
using Mashkoor.Modules.Customers.Domain;
using Mashkoor.Modules.Users.Commands;
using Mashkoor.Modules.Users.Queries;

namespace Mashkoor.Modules.Test.Users.Queries.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class ListDevicesTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_usersManager_role()
        => await AssertCommandAccess(TestQuery, [Roles.UsersManager]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_user_device_list()
    {
        // Arrange
        var manager = await InsertManagerAsync();
        var customer1 = await EnrollCustomer();
        var customer2 = await EnrollCustomer();

        var custDevice1 = (await FindAsync<Customer>(customer1.Id, "User.DeviceList")).User.DeviceList.Single();
        Assert.IsType<Created<IdObj>>(await SendAsync(new ReportDeviceError.Command(custDevice1.DeviceId, default, default, default, default, default, default, default, default, default, default, ReportDeviceError.Handler.Key)));

        var custDevice2 = (await FindAsync<Customer>(customer2.Id, "User.DeviceList")).User.DeviceList.Single();
        Assert.IsType<Created<IdObj>>(await SendAsync(new ReportDeviceError.Command(custDevice2.DeviceId, default, default, default, default, default, default, default, default, default, default, ReportDeviceError.Handler.Key)));

        // Act
        var result = await SendAsync(TestQuery with { Id = customer1.Id }, manager);

        // Assert
        var okResult = Assert.IsType<Ok<PagedQueryResponse<ListDevices.Response>>>(result);
        var response = okResult.Value;
        Assert.Equal(1, response.Total);

        var device = Assert.Single(response.Data);
        Assert.Equal(custDevice1.Id, device.Id);
        Assert.Equal(custDevice1.DeviceId, device.DeviceId);
        Assert.Equal(custDevice1.Manufacturer, device.Manufacturer);
        Assert.Equal(custDevice1.Model, device.Model);
        Assert.Equal(custDevice1.Platform, device.Platform);
        Assert.Equal(custDevice1.OSVersion, device.OSVersion);
        Assert.Equal(custDevice1.Idiom, device.Idiom);
        Assert.Equal(custDevice1.DeviceType, device.DeviceType);
        Assert.Equal(custDevice1.PnsHandle, device.Handle);
        Assert.Equal(custDevice1.RegisteredOn, device.RegisteredOn);
        Assert.Equal(custDevice1.AppVersion, device.AppVersion);
        Assert.Equal(custDevice1.IsStalled(DateTime.UtcNow), device.IsStalled);
        Assert.Equal(1, device.Faults);
    }

    private static ListDevices.Query TestQuery => new(1, null, null, null, null, null);
}
