using System.Globalization;
using Microsoft.AspNetCore.Http.HttpResults;
using Mashkoor.Core.Http;
using Mashkoor.Modules.Users.Commands.Responses;

namespace Mashkoor.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class SignInTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Accepted_but_wont_send_sms_when_user_doesnt_exist()
    {
        // Arrange
        var cmd = TestSignIn().Generate();
        var smsCallCount = 0;
        OnSms = (_, _) => smsCallCount++;

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var accepted = Assert.IsType<Accepted<OtpResponse>>(result);
        var response = accepted.Value;
        Assert.Equal(cmd.PhoneNumber, response.Username);
        Assert.Equal(0, smsCallCount);
    }

    // Can't test this as non-mfa users use email as username and this won't go through sign-in validator which required phone number username.
    //[SkippableFact(typeof(PlatformNotSupportedException))]
    //public async Task Returns_Accepted_but_wont_send_sms_when_non_mfa_user()
    //{
    //    // Arrange
    //    var cmd = TestSignIn(MfaUserType.Customer).Generate();
    //    await EnrollDispatcher(cmd.PhoneNumber);
    //    string otp = null;
    //    OnSms = (_, otp_) => otp = otp_;

    //    // Act
    //    var result = await SendAsync(cmd);

    //    // Assert
    //    var accepted = Assert.IsType<AcceptedResult>(result);
    //    var response = Assert.IsType<OtpResponse>(accepted.Value);
    //    Assert.Equal(cmd.PhoneNumber, response.Username);
    //    Assert.Equal("", response.Otp);
    //    Assert.Null(otp);
    //}

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Forbidden_when_user_is_banned()
    {
        // Arrange
        var cmd = TestSignIn().Generate();
        await EnrollCustomer(cmd.PhoneNumber, isBanned: true);

        // Act
        var result = await SendAsync(cmd);

        // Assert
        Assert.IsType<ForbiddenHttpResult<ProblemDetails>>(result);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Sends_otp_sms_when_customer_exist()
    {
        // Arrange
        var cmd = TestSignIn().Generate();
        await EnrollCustomer(cmd.PhoneNumber);
        var otp = "NULL";
        var smsCallCount = 0;
        OnSms = (_, otp_) => { otp = otp_; smsCallCount++; };

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var accepted = Assert.IsType<Accepted<OtpResponse>>(result);
        var response = accepted.Value;
        Assert.Equal(cmd.PhoneNumber, response.Username);
        Assert.NotEqual("NULL", otp);
        Assert.Equal(1, smsCallCount);
    }

    [SkippableTheory(typeof(PlatformNotSupportedException))]
    [InlineData("ar-SA", "رمز تحقق مشكور الخاص بك هو")]
    [InlineData("en-US", "Your Mashkoor verification code is")]
    [InlineData("ru-RU", "Ваш проверочный код Mashkoor")]
    public async Task Sends_localized_otp_sms(string lang, string expected)
    {
        // Arrange
        var cmd = TestSignIn().Generate();
        await EnrollCustomer(cmd.PhoneNumber);
        var otp = "NULL";
        OnSms = (_, otp_) => otp = otp_;

        // Act
        var defaultCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
        var result = await SendAsync(cmd);
        Thread.CurrentThread.CurrentCulture = defaultCulture;

        // Assert
        Assert.StartsWith(expected, otp);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Accepted_but_wont_send_sms_when_a_valid_code_still_exist()
    {
        // Arrange
        var cmd = TestSignIn().Generate();
        await EnrollCustomer(cmd.PhoneNumber);
        var smsCallCount = 0;
        OnSms = (_, _) => smsCallCount++;

        // Act
        await SendAsync(cmd);
        var result = await SendAsync(cmd);

        // Assert
        var accepted = Assert.IsType<Accepted<OtpResponse>>(result);
        var response = accepted.Value;
        Assert.Equal(cmd.PhoneNumber, response.Username);
        Assert.Equal(1, smsCallCount);
    }
}
