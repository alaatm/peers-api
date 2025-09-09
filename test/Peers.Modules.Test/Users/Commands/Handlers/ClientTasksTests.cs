using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Peers.Modules.Users.Commands;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class ClientTaskRequestTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_usersManager_role()
        => await AssertCommandAccess(new ClientTaskRequest.Command(default, default, default), [Roles.UsersManager]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_NotFound_when_device_does_not_exist()
    {
        // Arrange
        var manager = await InsertManagerAsync();
        var cmd = new ClientTaskRequest.Command(Guid.NewGuid(), ClientTaskRequest.RequiredTask.Ping, null);

        // Act
        var result = await SendAsync(cmd, manager);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_when_device_fcm_token_is_null()
    {
        // Arrange
        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();
        var device = customer.User.DeviceList.Single();
        ExecuteDbContext(db =>
        {
            var d = db.Set<Device>().Single(p => p.Id == device.Id);
            d.PnsHandle = null;
            db.SaveChanges();
        });

        var cmd = new ClientTaskRequest.Command(device.DeviceId, ClientTaskRequest.RequiredTask.Ping, null);

        // Act
        var result = await SendAsync(cmd, manager);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Device is not registered for push notifications.", problem.Detail);
    }

    [SkippableTheory(typeof(PlatformNotSupportedException))]
    [InlineData(ClientTaskRequest.RequiredTask.Ping)]
    [InlineData(ClientTaskRequest.RequiredTask.SignOff)]
    public async Task Returns_Accepted_and_sets_cache_entry_when_device_is_found(ClientTaskRequest.RequiredTask task)
    {
        // Arrange
        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();
        var device = customer.User.DeviceList.Single();

        var cmd = new ClientTaskRequest.Command(device.DeviceId, task, null);

        // Act
        var result = await SendAsync(cmd, manager);

        // Assert
        var response = Assert.IsType<Accepted<ClientTaskRequest.RequestResponse>>(result);
        var requestId = response.Value.RequestId;

        var cache = Services.GetRequiredService<IMemoryCache>();
        var key = $"{device.DeviceId}:{requestId}";
        var exists = cache.TryGetValue(key, out bool value);
        Assert.True(exists);
        Assert.False(value);
    }

    [SkippableTheory(typeof(PlatformNotSupportedException))]
    [InlineData(ClientTaskRequest.RequiredTask.Ping)]
    [InlineData(ClientTaskRequest.RequiredTask.SignOff)]
    public async Task Returns_Accepted_and_dispatches_notification_when_device_is_found(ClientTaskRequest.RequiredTask task)
    {
        var broadcastList = new List<(IReadOnlyList<Message>, IReadOnlyList<MulticastMessage>)>();
        OnPush = broadcastList.Add;

        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();
        var device = customer.User.DeviceList.Single();

        var cmd = new ClientTaskRequest.Command(device.DeviceId, task, null);

        // Act
        var result = await SendAsync(cmd, manager);

        // Assert
        var response = Assert.IsType<Accepted<ClientTaskRequest.RequestResponse>>(result);
        var requestId = response.Value.RequestId;

        var arg = Assert.Single(broadcastList);
        Assert.Empty(arg.Item2);
        var message = Assert.Single(arg.Item1);

        Assert.Null(message.Topic);
        Assert.Null(message.Notification);
        Assert.NotNull(message.Data);
        Assert.Equal("client-task", message.Data["event"]);
        Assert.Equal(task.ToString().ToUpperInvariant(), message.Data["task"]);
        Assert.Equal(requestId, message.Data["request-id"]);
        Assert.Equal(device.PnsHandle, message.Token);
    }

    [SkippableTheory(typeof(PlatformNotSupportedException))]
    [InlineData(ClientTaskRequest.AcknowledgeStatus.Waiting)]
    [InlineData(ClientTaskRequest.AcknowledgeStatus.Ok)]
    [InlineData(ClientTaskRequest.AcknowledgeStatus.NoResponse)]
    public async Task Returns_device_ping_status(ClientTaskRequest.AcknowledgeStatus status)
    {
        // Arrange
        var manager = await InsertManagerAsync();

        var cache = Services.GetRequiredService<IMemoryCache>();
        var deviceId = Guid.NewGuid();
        var requestId = "123";

        switch (status)
        {
            case ClientTaskRequest.AcknowledgeStatus.Waiting:
                cache.Set($"{deviceId}:{requestId}", false);
                break;
            case ClientTaskRequest.AcknowledgeStatus.Ok:
                cache.Set($"{deviceId}:{requestId}", true);
                break;
            case ClientTaskRequest.AcknowledgeStatus.NoResponse:
            default:
                break;
        }

        var cmd = new ClientTaskRequest.Command(deviceId, default, requestId);

        // Act
        var result = await SendAsync(cmd, manager);

        // Assert
        var okResult = Assert.IsType<Ok<ClientTaskRequest.AcknowledgeResponse>>(result);
        Assert.Equal(status, okResult.Value.Status);
    }
}

[Collection(nameof(IntegrationTestBaseCollection))]
public class ClientTaskAcknowledgeTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_no_auth()
        => await AssertCommandAccessNoAuth(new ClientTaskAcknowledge.Command(default, default));

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_NotFound_when_no_cache_entry_exists()
    {
        // Arrange
        var cmd = new ClientTaskAcknowledge.Command(Guid.NewGuid(), "123");

        // Act
        var result = await SendAsync(cmd);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_NoContent_and_sets_cache_entry_when_a_valid_request_exists()
    {
        // Arrange
        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();
        var device = customer.User.DeviceList.Single();

        var response = Assert.IsType<Accepted<ClientTaskRequest.RequestResponse>>(await SendAsync(new ClientTaskRequest.Command(device.DeviceId, ClientTaskRequest.RequiredTask.Ping, null), manager));
        var requestId = response.Value.RequestId;

        var cmd = new ClientTaskAcknowledge.Command(device.DeviceId, requestId);

        // Act
        var result = await SendAsync(cmd);

        // Assert
        Assert.IsType<NoContent>(result);
        var key = $"{device.DeviceId}:{requestId}";
        var cache = Services.GetRequiredService<IMemoryCache>();
        var exists = cache.TryGetValue(key, out bool value);
        Assert.True(exists);
        Assert.True(value);
    }
}
