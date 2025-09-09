using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Http.HttpResults;
using Peers.Modules.Users.Commands;

namespace Peers.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class DispatchMessageTests : IntegrationTestBase
{
    private static readonly Dictionary<string, string> _title = new() { { "en", "title" }, { "ar", "عنوان" }, { "ru", "заголовок" }, };
    private static readonly Dictionary<string, string> _body = new() { { "en", "body" }, { "ar", "هيكل" }, { "ru", "структура" }, };

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_usersManager_role()
        => await AssertCommandAccess(new DispatchMessage.Command(default, default, default), [Roles.UsersManager]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_when_title_is_missing_a_language()
    {
        // Arrange
        var manager = await InsertManagerAsync();

        // Act
        var result = await SendAsync(new DispatchMessage.Command("111", new() { { "en", "title" } }, _body), manager);

        // Assert
        var badRequest = AssertX.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Title must be set for all supported languages.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_when_body_is_missing_a_language()
    {
        // Arrange
        var manager = await InsertManagerAsync();

        // Act
        var result = await SendAsync(new DispatchMessage.Command("111", _title, new() { { "en", "title" } }), manager);

        // Assert
        var badRequest = AssertX.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Body must be set for all supported languages.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_when_dispatching_to_device_that_doesnt_have_a_user()
    {
        // Arrange
        var manager = await InsertManagerAsync();

        // Act
        var result = await SendAsync(new DispatchMessage.Command("111", _title, _body), manager);

        // Assert
        var badRequest = AssertX.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("User not found.", problem.Detail);
    }

    [SkippableTheory(typeof(PlatformNotSupportedException))]
    [InlineData("en")]
    [InlineData("ar")]
    public async Task Dispatches_message_to_device(string preferredLang)
    {
        // Arrange
        var manager = await InsertManagerAsync();
        var customer = await EnrollCustomer();
        var deviceToken = customer.User.DeviceList.Single().PnsHandle;
        var broadcastList = new List<(IReadOnlyList<Message>, IReadOnlyList<MulticastMessage>)>();
        OnPush = broadcastList.Add;

        AssertX.IsType<NoContent>(await SendAsync(new SetPreferredLanguage.Command(preferredLang), customer));

        // Act
        var result = await SendAsync(new DispatchMessage.Command(deviceToken, _title, _body), manager);

        // Assert
        AssertX.IsType<NoContent>(result);

        var arg = Assert.Single(broadcastList);
        Assert.Empty(arg.Item2);
        var message = Assert.Single(arg.Item1);

        Assert.Null(message.Data);
        Assert.Null(message.Topic);
        Assert.Equal(_title[preferredLang], message.Notification.Title);
        Assert.Equal(_body[preferredLang], message.Notification.Body);
        Assert.Equal(deviceToken, message.Token);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Dispatches_message_to_customers_group()
    {
        // Arrange
        var manager = await InsertManagerAsync();
        var broadcastList = new List<(IReadOnlyList<Message>, IReadOnlyList<MulticastMessage>)>();
        OnPush = broadcastList.Add;

        // Act
        var result = await SendAsync(new DispatchMessage.Command(null, _title, _body), manager);

        // Assert
        AssertX.IsType<NoContent>(result);

        var arg = Assert.Single(broadcastList);
        Assert.Empty(arg.Item2);
        Assert.Equal(3, arg.Item1.Count);

        var enMessage = arg.Item1.ElementAt(0);
        Assert.Null(enMessage.Data);
        Assert.Equal("customers-en", enMessage.Topic);
        Assert.Equal(_title["en"], enMessage.Notification.Title);
        Assert.Equal(_body["en"], enMessage.Notification.Body);
        Assert.Null(enMessage.Token);

        var arMessage = arg.Item1.ElementAt(1);
        Assert.Null(arMessage.Data);
        Assert.Equal("customers-ar", arMessage.Topic);
        Assert.Equal(_title["ar"], arMessage.Notification.Title);
        Assert.Equal(_body["ar"], arMessage.Notification.Body);
        Assert.Null(arMessage.Token);

        var ruMessage = arg.Item1.ElementAt(2);
        Assert.Null(arMessage.Data);
        Assert.Equal("customers-ru", ruMessage.Topic);
        Assert.Equal(_title["ru"], ruMessage.Notification.Title);
        Assert.Equal(_body["ru"], ruMessage.Notification.Body);
        Assert.Null(ruMessage.Token);
    }
}
