using Mashkoor.Modules.Users.Events;
using Mashkoor.Modules.Users.EventHandlers;
using Microsoft.Extensions.Caching.Memory;
using Mashkoor.Core.Communication.Sms;
using Mashkoor.Modules.Test.Common;
using static Mashkoor.Modules.Test.SharedClasses.MockBuilder;

namespace Mashkoor.Modules.Test.Users.EventHandlers;

public class OnEnrollRequestedTests
{
    [Theory]
    [InlineData("ar")]
    [InlineData("en")]
    [InlineData("ru")]
    public async Task Sends_localized_otp_sms_to_new_user(string langCode)
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var smsMoq = new Mock<ISmsService>(MockBehavior.Strict);
        var username = "+1234567890";
        var notification = new EnrollRequested(
            IdentityHelper.Get(),
            username,
            langCode);

        smsMoq.Setup(s => s.SendAsync(username, It.Is<string>(p => p.StartsWith($"{langCode}:")))).Returns(Task.CompletedTask);

        // Act
        await CreateHandler(cache, smsMoq.Object).Handle(notification, CancellationToken.None);

        // Assert
        Assert.NotEmpty(cache.Get<string>(username));
        smsMoq.Verify(s => s.SendAsync(username, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Does_not_sends_otp_message_to_new_user_when_a_still_valid_otp_was_already_sent()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var smsMoq = new Mock<ISmsService>(MockBehavior.Strict);
        var username = "+1234567890";
        var notification = new EnrollRequested(
            IdentityHelper.Get(),
            username,
            "en");

        cache.Set(username, "1234", TimeSpan.FromMinutes(3));

        // Act
        await CreateHandler(cache, smsMoq.Object).Handle(notification, CancellationToken.None);

        // Assert
        // No exceptions from strict mocks
    }

    private static OnEnrollRequested CreateHandler(
        IMemoryCache cache,
        ISmsService sms) => new(cache, sms, new SLCultureMoq<res>());
}
