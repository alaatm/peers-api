using Peers.Core.Communication.Sms;
using Peers.Modules.Test.Common;
using Peers.Modules.Users.EventHandlers;
using Peers.Modules.Users.Events;
using Microsoft.Extensions.Caching.Memory;
using static Peers.Modules.Test.SharedClasses.MockBuilder;

namespace Peers.Modules.Test.Users.EventHandlers;

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
        smsMoq.Verify(s => s.SendAsync(username, It.Is<string>(p => !p.EndsWith("1234"))), Times.Once);
    }

    [Fact]
    public async Task Sends_defaultOtp_sms_to_new_user_when_configured()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var smsMoq = new Mock<ISmsService>(MockBehavior.Strict);
        var username = "+1234567890";
        var notification = new EnrollRequested(
            IdentityHelper.Get(),
            username,
            "en");

        smsMoq.Setup(s => s.SendAsync(username, It.Is<string>(p => p.StartsWith("en:")))).Returns(Task.CompletedTask);

        // Act
        await CreateHandlerWithDefaultOtp(cache, smsMoq.Object).Handle(notification, CancellationToken.None);

        // Assert
        Assert.NotEmpty(cache.Get<string>(username));
        smsMoq.Verify(s => s.SendAsync(username, It.Is<string>(p => p.EndsWith("1234"))), Times.Once);
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
        ISmsService sms) => new(
            new() { UseDefaultOtp = false, Duration = TimeSpan.FromMinutes(3) },
            cache,
            sms,
            new SLCultureMoq<res>());

    private static OnEnrollRequested CreateHandlerWithDefaultOtp(
        IMemoryCache cache,
        ISmsService sms) => new(
            new() { UseDefaultOtp = true, DefaultOtp = "1234", Duration = TimeSpan.FromMinutes(3) },
            cache,
            sms,
            new SLCultureMoq<res>());
}
