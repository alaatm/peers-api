using FirebaseAdmin.Messaging;
using Mashkoor.Core.Communication.Push;
using Mashkoor.Modules.Users;
using Mashkoor.Modules.Users.Domain;

namespace Mashkoor.Modules.Test.Users;

public class FirebaseMessagingServiceExtensionsTests
{
    [Fact]
    public async Task SubscribeUserTopicAsync_noops_and_returns_null_when_null_token()
    {
        // Arrange
        var user = new AppUser();
        var firebaseMoq = new Mock<IFirebaseMessagingService>(MockBehavior.Strict);

        // Act
        var result = await firebaseMoq.Object.SubscribeUserTopicAsync(user, null);

        // Assert
        Assert.Null(result);
        firebaseMoq.VerifyAll();
    }

    [Theory]
    [InlineData("en", "customers-en")]
    [InlineData("ar", "customers-ar")]
    [InlineData("ru", "customers-ru")]
    public async Task SubscribeUserTopicAsync_invokes_subscription_to_appropriate_topic_depending_on_userType_and_preferredLang(string preferredLang, string expectedTopic)
    {
        // Arrange
        var user = new AppUser { PreferredLanguage = preferredLang };
        var handle = Guid.NewGuid().ToString();
        var firebaseMoq = new Mock<IFirebaseMessagingService>(MockBehavior.Strict);
        firebaseMoq.Setup(p => p.SubscribeToTopicAsync(handle, expectedTopic)).ReturnsAsync((TopicManagementResponse)null).Verifiable();

        // Act
        await firebaseMoq.Object.SubscribeUserTopicAsync(user, handle);

        // Assert
        firebaseMoq.VerifyAll();
    }

    [Fact]
    public async Task UnsubscribeUserTopicAsync_noops_and_returns_null_when_null_token()
    {
        // Arrange
        var user = new AppUser();
        var firebaseMoq = new Mock<IFirebaseMessagingService>(MockBehavior.Strict);

        // Act
        var result = await firebaseMoq.Object.UnsubscribeUserTopicAsync(user, null);

        // Assert
        Assert.Null(result);
        firebaseMoq.VerifyAll();
    }

    [Theory]
    [InlineData("en", "customers-en")]
    [InlineData("ar", "customers-ar")]
    [InlineData("ru", "customers-ru")]
    public async Task UnsubscribeUserTopicAsync_invokes_subscription_to_appropriate_topic_depending_on_userType_and_preferredLang(string preferredLang, string expectedTopic)
    {
        // Arrange
        var user = new AppUser { PreferredLanguage = preferredLang };
        var handle = Guid.NewGuid().ToString();
        var firebaseMoq = new Mock<IFirebaseMessagingService>(MockBehavior.Strict);
        firebaseMoq.Setup(p => p.UnsubscribeFromTopicAsync(handle, expectedTopic)).ReturnsAsync((TopicManagementResponse)null).Verifiable();

        // Act
        await firebaseMoq.Object.UnsubscribeUserTopicAsync(user, handle);

        // Assert
        firebaseMoq.VerifyAll();
    }
}
