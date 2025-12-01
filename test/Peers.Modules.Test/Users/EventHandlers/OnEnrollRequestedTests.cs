using System.Text.RegularExpressions;
using Peers.Core.Communication.Sms;
using Peers.Modules.Test.Common;
using Peers.Modules.Users.EventHandlers;
using Peers.Modules.Users.Events;
using Microsoft.Extensions.Caching.Memory;
using static Peers.Modules.Test.SharedClasses.MockBuilder;

namespace Peers.Modules.Test.Users.EventHandlers;

public class OnEnrollRequestedTests
{
    private const string DefaultOtp = "1234";

    [Theory]
    [InlineData("ar")]
    [InlineData("en")]
    [InlineData("ru")]
    public async Task Sends_localized_otp_sms_to_new_user(string langCode)
    {
        // Arrange
        var regex = new Regex($@"^{langCode}:Your Peers verification code is: (?!{DefaultOtp}$)\d{{4}}$"); // not 1234 (default OTP)
        var cache = new MemoryCache(new MemoryCacheOptions());
        var smsMoq = new Mock<ISmsService>(MockBehavior.Strict);
        var username = TestUsername();
        var phoneNumber = TestPhoneNumber();
        var notification = new EnrollRequested(
            IdentityHelper.Get(),
            username,
            phoneNumber,
            langCode);

        smsMoq
            .Setup(s => s.SendAsync(phoneNumber, It.Is<string>(p => regex.IsMatch(p))))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await CreateHandler(cache, smsMoq.Object).Handle(notification, CancellationToken.None);

        // Assert
        Assert.NotEmpty(cache.Get<string>($"{username}:{phoneNumber}"));
        smsMoq.Verify(s => s.SendAsync(phoneNumber, It.IsAny<string>()), Times.Once);
        smsMoq.VerifyAll();
    }

    [Fact]
    public async Task Sends_defaultOtp_sms_to_new_user_when_configured()
    {
        // Arrange
        var cache = new MemoryCache(new MemoryCacheOptions());
        var smsMoq = new Mock<ISmsService>(MockBehavior.Strict);
        var username = TestUsername();
        var phoneNumber = TestPhoneNumber();
        var notification = new EnrollRequested(
            IdentityHelper.Get(),
            username,
            phoneNumber,
            "en");

        smsMoq
            .Setup(s => s.SendAsync(phoneNumber, $"en:Your Peers verification code is: {DefaultOtp}"))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await CreateHandlerWithDefaultOtp(cache, smsMoq.Object).Handle(notification, CancellationToken.None);

        // Assert
        Assert.NotEmpty(cache.Get<string>($"{username}:{phoneNumber}"));
        smsMoq.Verify(s => s.SendAsync(phoneNumber, It.IsAny<string>()), Times.Once);
        smsMoq.VerifyAll();
    }

    [Fact]
    public async Task Does_not_sends_otp_message_to_new_user_when_a_still_valid_otp_was_already_sent()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var smsMoq = new Mock<ISmsService>(MockBehavior.Strict);
        var username = TestUsername();
        var phoneNumber = TestPhoneNumber();
        var notification = new EnrollRequested(
            IdentityHelper.Get(),
            username,
            phoneNumber,
            "en");

        cache.Set($"{username}:{phoneNumber}", "1234", TimeSpan.FromMinutes(3));

        // Act
        await CreateHandler(cache, smsMoq.Object).Handle(notification, CancellationToken.None);

        // Assert
        smsMoq.Verify(s => s.SendAsync(phoneNumber, It.IsAny<string>()), Times.Never);
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
            new() { UseDefaultOtp = true, DefaultOtp = DefaultOtp, Duration = TimeSpan.FromMinutes(3) },
            cache,
            sms,
            new SLCultureMoq<res>());
}
