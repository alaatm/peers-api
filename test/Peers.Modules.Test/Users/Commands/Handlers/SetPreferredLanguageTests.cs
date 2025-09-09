using Microsoft.AspNetCore.Http.HttpResults;
using Peers.Core.Commands;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Users.Commands;

namespace Peers.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class SetPreferredLanguageTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_customer_role()
        => await AssertCommandAccess(new SetPreferredLanguage.Command(null), [Roles.Customer]);

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_badRequest_when_setting_a_non_supported_language()
    {
        // Arrange
        var lang = "zh";
        var customer = await EnrollCustomer();

        // Act
        var result = await SendAsync(new SetPreferredLanguage.Command(lang), customer);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Invalid language code.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Updates_user_preferred_language()
    {
        // Arrange
        var lang = "ar";
        var customer = await EnrollCustomer();

        // Act
        var result = await SendAsync(new SetPreferredLanguage.Command(lang), customer);

        // Assert
        Assert.IsType<NoContent>(result);
        var user = (await FindAsync<Customer>(customer.Id, "User")).User;
        Assert.Equal(lang, user.PreferredLanguage);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Unsubscribes_from_old_localized_topic_and_subscribes_to_new_localized_topic_for_all_user_devices()
    {
        // Arrange
        var lang = "ar";
        var customer = await EnrollCustomer();
        Assert.IsType<Created<IdObj>>(await SendAsync(TestRegisterDevice.Generate(), customer));
        var deviceToken1 = customer.User.DeviceList.First().PnsHandle;
        var deviceToken2 = customer.User.DeviceList.Last().PnsHandle;
        FirebaseMoq.Setup(p => p.UnsubscribeFromTopicAsync(deviceToken1, "customers-en")).Verifiable();
        FirebaseMoq.Setup(p => p.SubscribeToTopicAsync(deviceToken1, $"customers-{lang}")).Verifiable();
        FirebaseMoq.Setup(p => p.UnsubscribeFromTopicAsync(deviceToken2, "customers-en")).Verifiable();
        FirebaseMoq.Setup(p => p.SubscribeToTopicAsync(deviceToken2, $"customers-{lang}")).Verifiable();

        // Act
        var result = await SendAsync(new SetPreferredLanguage.Command(lang), customer);

        // Assert
        Assert.IsType<NoContent>(result);
        FirebaseMoq.VerifyAll();
        FirebaseMoq.Reset();
    }
}
