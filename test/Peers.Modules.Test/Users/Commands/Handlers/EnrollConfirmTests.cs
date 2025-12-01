using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Peers.Core.Identity;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Users.Commands.Responses;
using Peers.Modules.Users.Domain;
using Peers.Modules.Users.Events;
using System.Globalization;

namespace Peers.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class EnrollConfirmTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_when_already_authenticated()
    {
        // Arrange
        var customer = await EnrollCustomer();
        var cmd = TestEnrollConfirm().Generate();

        // Act
        var result = await SendAsync(cmd, customer);

        // Assert
        var unauthResult = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(unauthResult.Value);
        Assert.Equal("You are already authenticated.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Conflict_when_username_already_exist()
    {
        // Arrange
        var cmd = TestEnrollConfirm().Generate();
        await EnrollCustomer(username: cmd.Username);

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var conflict = Assert.IsType<Conflict<ProblemDetails>>(result);
        var problem = conflict.Value;
        Assert.Equal("Username or phone number already exist.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Conflict_when_phoneNumber_already_exist()
    {
        // Arrange
        var cmd = TestEnrollConfirm().Generate();
        await EnrollCustomer(phoneNumber: cmd.PhoneNumber);

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var conflict = Assert.IsType<Conflict<ProblemDetails>>(result);
        var problem = conflict.Value;
        Assert.Equal("Username or phone number already exist.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_when_invalid_otp()
    {
        // Arrange
        var cmd = TestEnrollConfirm().Generate();
        await SendAsync(TestEnroll().Generate() with { Username = cmd.Username });

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var badRequest = Assert.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Invalid verification code.", problem.Detail);
    }
}

[Collection(nameof(IntegrationTestBaseCollection))]
public class EnrollConfirmMfaTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Creates_user_and_returns_token()
    {
        // Arrange
        var cmd = TestEnrollConfirm().Generate();
        var userSecret = Guid.NewGuid().ToString("N");

        ProducerMoq
            .Setup(p => p.PublishAsync(It.IsAny<EnrollRequested>(), It.IsAny<CancellationToken>()))
            .Callback(() =>
                Services.GetRequiredService<IMemoryCache>().Set($"{cmd.Username}:{cmd.PhoneNumber}", DefaultOtp, TimeSpan.FromDays(1)));

        HmacHashMoq
            .Setup(p => p.GenerateKey())
            .Returns(userSecret);

        await SendAsync(TestEnroll().Generate() with { Username = cmd.Username, PhoneNumber = cmd.PhoneNumber });

        // Act
        var result = await SendAsync(cmd with { Otp = DefaultOtp });

        // Assert
        var objResult = Assert.IsType<Ok<JwtResponse>>(result);
        var response = objResult.Value;
        Assert.Equal(cmd.Username, response.Username);
        Assert.NotEmpty(response.Token);

        var customer = await FindAsync<Customer>(p => p.Username == cmd.Username, "User.RefreshTokens");
        var user = customer.User;
        Assert.Equal(userSecret, customer.Secret);
        Assert.Equal(cmd.PreferredLanguage, user.PreferredLanguage);
        Assert.Equal(user.RefreshTokens.Single().Token, response.RefreshToken);
        Assert.Equal(JwtDuration, Math.Round((response.TokenExpiry - DateTime.UtcNow).TotalMinutes));

        ProducerMoq.VerifyAll();
        HmacHashMoq.VerifyAll();
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Assigns_customer_role_and_claim_when_enrolling_customer()
    {
        // Arrange
        var cmd = TestEnrollConfirm().Generate();

        ProducerMoq.Setup(p => p.PublishAsync(It.IsAny<EnrollRequested>(), It.IsAny<CancellationToken>())).Callback(() =>
            Services.GetRequiredService<IMemoryCache>().Set($"{cmd.Username}:{cmd.PhoneNumber}", DefaultOtp, TimeSpan.FromDays(1)));

        HmacHashMoq
            .Setup(p => p.GenerateKey())
            .Returns(Guid.NewGuid().ToString("N"));

        await SendAsync(TestEnroll().Generate() with { Username = cmd.Username, PhoneNumber = cmd.PhoneNumber });

        // Act
        var result = await SendAsync(cmd with { Otp = DefaultOtp });

        // Assert
        var objResult = Assert.IsType<Ok<JwtResponse>>(result);
        var createdUser = await FindAsync<AppUser>(p => p.UserName == cmd.Username);

        var expectedRoles = new[] { Roles.Customer };
        var expectedClaims = new Claim[]
        {
            new(CustomClaimTypes.Id, createdUser.Id.ToString(CultureInfo.InvariantCulture)),
            new(CustomClaimTypes.Username, cmd.Username),
        };

        await AssertAssignedRolesAndClaimsAsync(createdUser, expectedRoles, expectedClaims);
        AssertAssignedRolesAndClaimsInJwt(objResult.Value.Token, createdUser, expectedRoles, expectedClaims);
    }
}
