using System.Reflection;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Mashkoor.Core.Communication.Push;
using Mashkoor.Core.Communication.Push.Configuration;

namespace Mashkoor.Core.Test.Communication.Push;

public class FirebaseMessagingServiceTests
{
    public const string TestFirebaseServiceAccount = /*lang=json,strict*/ @"{
  ""type"": ""service_account"",
  ""project_id"": ""test"",
  ""private_key_id"": ""xxx"",
  ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIBVgIBADANBgkqhkiG9w0BAQEFAASCAUAwggE8AgEAAkEAq7BFUpkGp3+LQmlQ\nYx2eqzDV+xeG8kx/sQFV18S5JhzGeIJNA72wSeukEPojtqUyX2J0CciPBh7eqclQ\n2zpAswIDAQABAkAgisq4+zRdrzkwH1ITV1vpytnkO/NiHcnePQiOW0VUybPyHoGM\n/jf75C5xET7ZQpBe5kx5VHsPZj0CBb3b+wSRAiEA2mPWCBytosIU/ODRfq6EiV04\nlt6waE7I2uSPqIC20LcCIQDJQYIHQII+3YaPqyhGgqMexuuuGx+lDKD6/Fu/JwPb\n5QIhAKthiYcYKlL9h8bjDsQhZDUACPasjzdsDEdq8inDyLOFAiEAmCr/tZwA3qeA\nZoBzI10DGPIuoKXBd3nk/eBxPkaxlEECIQCNymjsoI7GldtujVnr1qT+3yedLfHK\nsrDVjIT3LsvTqw==\n-----END PRIVATE KEY-----\n"",
  ""client_email"": ""firebase-adminsdk-xxxxxx@test.iam.gserviceaccount.com"",
  ""client_id"": ""xxx"",
  ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
  ""token_uri"": ""https://oauth2.googleapis.com/token"",
  ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
  ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-xxxxx%40test.iam.gserviceaccount.com""
}";

    private static readonly FirebaseConfig _config = new() { ServiceAccountKey = TestFirebaseServiceAccount };

    [Fact]
    public void Ctor_throws_when_firebaseServiceKey_is_not_set_on_the_tenant()
    {
        // Arrange
        var wrapperMoq = new Mock<IFirebaseMessagingWrapper>();

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => new FirebaseMessagingService(new FirebaseConfig(), wrapperMoq.Object));

        // Assert
        Assert.Equal("Firebase service account key is not set.", ex.Message);
    }

    [Fact]
    public void Ctor_sets_inner_client_from_config()
    {
        // Arrange
        var wrapperMoq = new Mock<IFirebaseMessagingWrapper>();

        // Act
        var service = new FirebaseMessagingService(_config, wrapperMoq.Object);

        // Assert
        var expectedClient = FirebaseMessaging.GetMessaging(FirebaseApp.GetInstance(FirebaseMessagingService.FirebaseAppName));
        wrapperMoq.VerifySet(p => p.Client = expectedClient, Times.Once());
    }

    [Fact]
    public async Task Ctor_handles_multiple_concurrent_calls()
    {
        // Arrange
        var wrapperMoq = new Mock<IFirebaseMessagingWrapper>();

        // Act
        var tasks = Enumerable.Range(0, 500).Select(_ => Task.Run(() => new FirebaseMessagingService(_config, wrapperMoq.Object))).ToArray();
        await Task.WhenAll(tasks);

        // Assert
        var expectedClient = FirebaseMessaging.GetMessaging(FirebaseApp.GetInstance(FirebaseMessagingService.FirebaseAppName));
        wrapperMoq.VerifySet(p => p.Client = expectedClient, Times.Exactly(500));
    }

    [Fact]
    public async Task SendAsync_forwards_call_to_inner_client()
    {
        // Arrange
        var wrapperMoq = GetWrapperMoq();
        wrapperMoq.Setup(p => p.SendAsync(It.Is<Message>(p => p.Topic == "TestTopic"))).ReturnsAsync("messageId");
        var service = new FirebaseMessagingService(_config, wrapperMoq.Object);

        // Act
        var result = await service.SendAsync(new Message { Topic = "TestTopic" });

        // Assert
        wrapperMoq.VerifyAll();
    }

    [Fact]
    public async Task SendAsync_returns_sendResponse_with_messageId_on_success()
    {
        // Arrange
        var wrapperMoq = GetWrapperMoq();
        wrapperMoq.Setup(p => p.SendAsync(It.IsAny<Message>())).ReturnsAsync("messageId");
        var service = new FirebaseMessagingService(_config, wrapperMoq.Object);

        // Act
        var result = await service.SendAsync(new Message());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Exception);
        Assert.Equal("messageId", result.MessageId);
        wrapperMoq.VerifyAll();
    }

    [Fact]
    public async Task SendAsync_returns_sendResponse_with_exception_on_failure()
    {
        // Arrange
        var wrapperMoq = GetWrapperMoq();
        var ex = CreateException();
        wrapperMoq.Setup(p => p.SendAsync(It.IsAny<Message>())).ThrowsAsync(ex);
        var service = new FirebaseMessagingService(_config, wrapperMoq.Object);

        // Act
        var result = await service.SendAsync(new Message());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Same(ex, result.Exception);
        Assert.Null(result.MessageId);
        wrapperMoq.VerifyAll();
    }

    [Fact]
    public async Task SendAllAsync_forwards_call_to_inner_client()
    {
        // Arrange
        var wrapperMoq = GetWrapperMoq();
        wrapperMoq.Setup(p => p.SendEachAsync(It.Is<IEnumerable<Message>>(p => p.All(p => p.Topic == "TestTopic")))).ReturnsAsync(It.IsAny<BatchResponse>());
        var service = new FirebaseMessagingService(_config, wrapperMoq.Object);

        // Act
        var result = await service.SendAllAsync(
        [
            new Message { Topic = "TestTopic" },
            new Message { Topic = "TestTopic" },
        ]);

        // Assert
        wrapperMoq.VerifyAll();
    }

    [Fact]
    public async Task SendMulticastAsync_forwards_call_to_inner_client()
    {
        // Arrange
        var wrapperMoq = GetWrapperMoq();
        var multicastMessage = new MulticastMessage();
        wrapperMoq.Setup(p => p.SendEachForMulticastAsync(It.Is<MulticastMessage>(p => p == multicastMessage))).ReturnsAsync(It.IsAny<BatchResponse>());
        var service = new FirebaseMessagingService(_config, wrapperMoq.Object);

        // Act
        var result = await service.SendMulticastAsync(multicastMessage);

        // Assert
        wrapperMoq.VerifyAll();
    }

    [Fact]
    public async Task SubscribeToTopicAsync_forwards_call_to_inner_client()
    {
        // Arrange
        var wrapperMoq = GetWrapperMoq();
        var token = "token";
        wrapperMoq.Setup(p => p.SubscribeToTopicAsync(It.Is<IReadOnlyList<string>>(p => p[0] == token), "TestTopic")).ReturnsAsync(It.IsAny<TopicManagementResponse>());
        var service = new FirebaseMessagingService(_config, wrapperMoq.Object);

        // Act
        var result = await service.SubscribeToTopicAsync(token, "TestTopic");

        // Assert
        wrapperMoq.VerifyAll();
    }

    [Fact]
    public async Task UnsubscribeFromTopicAsync_forwards_call_to_inner_client()
    {
        // Arrange
        var wrapperMoq = GetWrapperMoq();
        var token = "token";
        wrapperMoq.Setup(p => p.UnsubscribeFromTopicAsync(It.Is<IReadOnlyList<string>>(p => p[0] == token), "TestTopic")).ReturnsAsync(It.IsAny<TopicManagementResponse>());
        var service = new FirebaseMessagingService(_config, wrapperMoq.Object);

        // Act
        var result = await service.UnsubscribeFromTopicAsync(token, "TestTopic");

        // Assert
        wrapperMoq.VerifyAll();
    }

    private static Mock<IFirebaseMessagingWrapper> GetWrapperMoq()
    {
        var wrapperMoq = new Mock<IFirebaseMessagingWrapper>(MockBehavior.Strict);
        wrapperMoq.SetupSet(p => p.Client = It.IsAny<FirebaseMessaging>());
        return wrapperMoq;
    }

    private static FirebaseMessagingException CreateException()
    {
        var ctor = typeof(FirebaseMessagingException).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single();
        return (FirebaseMessagingException)ctor.Invoke([ErrorCode.Unknown, "err", null, null, null]);
    }
}
