using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Peers.Modules.Users.Commands;
using Peers.Modules.Users.Domain;
using Peers.Modules.Users.Queries;

namespace Peers.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class SendEmailVerificationTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_auth()
        => await AssertCommandAccess(new SendEmailVerification.Command());

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_NoContent_when_email_already_confirmed()
    {
        // Arrange
        var user = (await EnrollCustomer()).User;
        await SetEmailConfirmed(user);

        // Act
        var result = await SendAsync(new SendEmailVerification.Command(), user);

        // Assert
        Assert.IsType<NoContent>(result);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_when_updatedEmail_is_null()
    {
        // Arrange
        var user = (await EnrollCustomer()).User;

        // Act
        var result = await SendAsync(new SendEmailVerification.Command(), user);

        // Assert
        var badResult = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badResult.Value);
        Assert.Equal("Email address is not set.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_ProblemObjectResult_when_link_generation_fails()
    {
        // Arrange
        HttpContextAccessorMoq.SetupGet(p => p.HttpContext).Returns(new DefaultHttpContext());
        LinkGeneratorMoq.Setup(p => p.GetUriByAddress(
            It.IsAny<HttpContext>(),
            It.IsAny<RouteValuesAddress>(),
            It.IsAny<RouteValueDictionary>(),
            null,
            null,
            null,
            null,
            It.IsAny<FragmentString>(),
            It.IsAny<LinkOptions>())).Returns((string)null);
        var user = (await EnrollCustomer()).User;
        Assert.IsType<Ok<GetProfile.Response>>(await SendAsync(new SetProfile.Command(null, null, "new@example.com", null), user));

        // Act
        var result = await SendAsync(new SendEmailVerification.Command(), user);

        // Assert
        HttpContextAccessorMoq.VerifyAll();
        LinkGeneratorMoq.VerifyAll();
        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal("Could not generate email verification link.", problem.ProblemDetails.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_noContent_when_success()
    {
        // Arrange
        HttpContextAccessorMoq.SetupGet(p => p.HttpContext).Returns(new DefaultHttpContext());
        LinkGeneratorMoq.Setup(p => p.GetUriByAddress(
            It.IsAny<HttpContext>(),
            It.IsAny<RouteValuesAddress>(),
            It.IsAny<RouteValueDictionary>(),
            null,
            null,
            null,
            null,
            It.IsAny<FragmentString>(),
            It.IsAny<LinkOptions>())).Returns("abs/path");
        var user = (await EnrollCustomer()).User;
        Assert.IsType<Ok<GetProfile.Response>>(await SendAsync(new SetProfile.Command(null, null, "new@example.com", null), user));

        // Act
        var result = await SendAsync(new SendEmailVerification.Command(), user);

        // Assert
        HttpContextAccessorMoq.VerifyAll();
        LinkGeneratorMoq.VerifyAll();
        Assert.IsType<NoContent>(result);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Sends_confirmation_email_when_success()
    {
        // Arrange
        string subject, body, recipient;
        subject = body = recipient = null;
        OnEmail = (subject_, body_, recipient_) => (subject, body, recipient) = (subject_, body_, recipient_);

        HttpContextAccessorMoq.SetupGet(p => p.HttpContext).Returns(new DefaultHttpContext());
        LinkGeneratorMoq.Setup(p => p.GetUriByAddress(
            It.IsAny<HttpContext>(),
            It.IsAny<RouteValuesAddress>(),
            It.IsAny<RouteValueDictionary>(),
            null,
            null,
            null,
            null,
            It.IsAny<FragmentString>(),
            It.IsAny<LinkOptions>())).Returns("abs/path");
        var user = (await EnrollCustomer()).User;
        Assert.IsType<Ok<GetProfile.Response>>(await SendAsync(new SetProfile.Command(null, null, "new@example.com", null), user));

        // Act
        Assert.IsType<NoContent>(await SendAsync(new SendEmailVerification.Command(), user));

        // Assert
        HttpContextAccessorMoq.VerifyAll();
        LinkGeneratorMoq.VerifyAll();
        Assert.Equal("Please confirm your new email address", subject);
        Assert.Contains(user.Firstname, body);
        Assert.Contains("new@example.com", body);
        Assert.Contains(user.PhoneNumber.Replace("+", ""), body);
        Assert.Contains("abs/path", body);
        Assert.Equal("new@example.com", recipient);
    }

    private Task SetEmailConfirmed(AppUser user) => ExecuteDbContextAsync(async db =>
    {
        user = await db.Users.FindAsync(user.Id);
        user.Email = "current@example.com";
        user.EmailConfirmed = true;
        await db.SaveChangesAsync();
    });
}
