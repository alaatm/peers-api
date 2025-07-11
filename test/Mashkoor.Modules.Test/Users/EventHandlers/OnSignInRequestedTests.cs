using Mashkoor.Core.Communication.Sms;
using Mashkoor.Core.Identity;
using Mashkoor.Core.Security.Totp;
using Mashkoor.Modules.Users.Domain;
using Mashkoor.Modules.Users.EventHandlers;
using Mashkoor.Modules.Users.Events;
using static Mashkoor.Modules.Test.SharedClasses.MockBuilder;

namespace Mashkoor.Modules.Test.Users.EventHandlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class OnSignInRequestedTests : IntegrationTestBase
{
    [SkippableTheory(typeof(PlatformNotSupportedException))]
    [InlineData("en")]
    [InlineData("ar")]
    [InlineData("ru")]
    public async Task Sends_localized_otp_sms_to_user(string langCode)
    {
        // Arrange
        var customer = await EnrollCustomer();
        var totpProviderMoq = new Mock<ITotpTokenProvider>(MockBehavior.Strict);
        var smsMoq = new Mock<ISmsService>(MockBehavior.Strict);
        var otp = "1234";

        totpProviderMoq
            .Setup(p => p.TryGenerate(
                It.Is<AppUser>(p => p.Id == customer.Id),
                TotpPurpose.SignInPurpose,
                out otp))
            .Returns(true)
            .Verifiable();

        smsMoq
            .Setup(s => s.SendAsync(customer.Username, $"{langCode}:Your Mashkoor verification code is: {otp}"))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await ExecuteDbContextAsync(async context =>
        {
            var handler = new OnSignInRequested(context, totpProviderMoq.Object, smsMoq.Object, new SLCultureMoq<res>());
            await handler.Handle(new SignInRequested(Mock.Of<IIdentityInfo>(), "platform", customer.Username, langCode), default);
        });

        // Assert
        totpProviderMoq.VerifyAll();
        smsMoq.VerifyAll();
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Does_not_sends_otp_message_when_a_still_valid_otp_was_already_sent()
    {
        // Arrange
        var customer = await EnrollCustomer();
        var totpProviderMoq = new Mock<ITotpTokenProvider>(MockBehavior.Strict);
        var smsMoq = new Mock<ISmsService>(MockBehavior.Strict);
        var otp = "1234";

        totpProviderMoq
            .Setup(p => p.TryGenerate(
                It.Is<AppUser>(p => p.Id == customer.Id),
                TotpPurpose.SignInPurpose,
                out otp))
            .Returns(false)
            .Verifiable();

        // Act
        await ExecuteDbContextAsync(async context =>
        {
            var handler = new OnSignInRequested(context, totpProviderMoq.Object, smsMoq.Object, new SLCultureMoq<res>());
            await handler.Handle(new SignInRequested(Mock.Of<IIdentityInfo>(), "platform", customer.Username, "en"), default);
        });

        // Assert
        totpProviderMoq.VerifyAll();
        smsMoq.VerifyAll();
    }
}
