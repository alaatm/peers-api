using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Mashkoor.Core.Http;
using Mashkoor.Core.Identity;
using Mashkoor.Modules.Customers.Domain;
using Mashkoor.Modules.Users.Commands;
using Mashkoor.Modules.Users.Commands.Responses;
using Mashkoor.Modules.Users.Domain;
using Microsoft.Extensions.DependencyInjection;
using Mashkoor.Core.Security.Totp;

namespace Mashkoor.Modules.Test.Users.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class CreateTokenTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Unauthorized_when_user_doesnt_exist_mfaGrant()
    {
        // Arrange
        var cmd = TestCreateToken(CreateToken.GrantType.Mfa).Generate();

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var unauthResult = AssertX.IsType<UnauthorizedHttpResult2<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(unauthResult.Value);
        Assert.Equal("Invalid verification code.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Unauthorized_when_user_doesnt_exist_passwordGrant()
    {
        // Arrange
        var cmd = TestCreateToken(CreateToken.GrantType.Password).Generate();

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var unauthResult = AssertX.IsType<UnauthorizedHttpResult2<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(unauthResult.Value);
        Assert.Equal("Username or password incorrect.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Unauthorized_when_user_doesnt_exist_refreshTokenGrant()
    {
        // Arrange
        var cmd = TestCreateToken(CreateToken.GrantType.RefreshToken).Generate();

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var unauthResult = AssertX.IsType<UnauthorizedHttpResult2<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(unauthResult.Value);
        Assert.Equal("Unable to refresh session.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Forbidden_when_user_is_banned()
    {
        // Arrange
        var cmd = TestCreateToken(CreateToken.GrantType.Mfa).Generate();
        await EnrollCustomer(cmd.Username, isBanned: true);

        // Act
        var result = await SendAsync(cmd);

        // Assert
        Assert.IsType<ForbiddenHttpResult<ProblemDetails>>(result);
    }
}

[Collection(nameof(IntegrationTestBaseCollection))]
public class CreateTokenMfaTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Unauthorized_when_invalid_otp()
    {
        // Arrange
        var cmd = TestCreateToken(CreateToken.GrantType.Mfa).Generate();
        await EnrollCustomer(cmd.Username);

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var unauthResult = AssertX.IsType<UnauthorizedHttpResult2<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(unauthResult.Value);
        Assert.Equal("Invalid verification code.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Issues_jwt_and_returns_token()
    {
        // Arrange
        string otp = null;
        var cmd = TestCreateToken(CreateToken.GrantType.Mfa).Generate();
        var customer = await EnrollCustomer(cmd.Username);

        ExecuteScope(sp =>
        {
            var totpTokenProvider = sp.GetRequiredService<ITotpTokenProvider>();
            totpTokenProvider.TryGenerate(customer.User, TotpPurpose.SignInPurpose, out otp);
        });

        // Act
        var result = await SendAsync(cmd with { Password = otp });

        // Assert
        var objResult = AssertX.IsType<Ok<JwtResponse>>(result);
        var response = objResult.Value;
        var user = (await FindAsync<Customer>(p => p.Username == cmd.Username, "User.RefreshTokens")).User;
        Assert.Equal(user.Firstname, response.Name);
        Assert.Equal(cmd.Username, response.Username);
        Assert.NotEmpty(response.Token);
        Assert.Equal(user.RefreshTokens.Single().Token, response.RefreshToken);
        Assert.Equal(JwtDuration, Math.Round((response.TokenExpiry - DateTime.UtcNow).TotalMinutes));

        AssertAssignedRolesAndClaimsInJwt(objResult.Value.Token, user, [Roles.Customer], GetExpectedClaims(user));

        static Claim[] GetExpectedClaims(AppUser user) =>
        [
            new(CustomClaimTypes.Id, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(CustomClaimTypes.Username, user.UserName!),
        ];
    }
}

[Collection(nameof(IntegrationTestBaseCollection))]
public class CreateTokenPasswordTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Unauthorized_when_password_incorrect()
    {
        // Arrange
        var cmd = TestCreateToken(CreateToken.GrantType.Password).Generate();
        await InsertManagerAsync(cmd.Username);

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var unauthResult = AssertX.IsType<UnauthorizedHttpResult2<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(unauthResult.Value);
        Assert.Equal("Username or password incorrect.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Issues_jwt_and_returns_token_staff()
    {
        // Arrange
        var cmd = TestCreateToken(CreateToken.GrantType.Password).Generate();
        var manager = await InsertManagerAsync(roles: Roles.MaintenanceAdmin);

        // Act
        var result = await SendAsync(cmd with { Username = manager.UserName, Password = "P@ssword" });

        // Assert
        var objResult = AssertX.IsType<Ok<JwtResponse>>(result);
        var response = objResult.Value;
        var user = await FindAsync<AppUser>(p => p.UserName == manager.UserName, "RefreshTokens");
        Assert.Equal(user.Firstname, response.Name);
        Assert.Equal(manager.UserName, response.Username);
        Assert.NotEmpty(response.Token);
        Assert.Equal(user.RefreshTokens.Single().Token, response.RefreshToken);
        Assert.Equal(JwtDuration, Math.Round((response.TokenExpiry - DateTime.UtcNow).TotalMinutes));

        var createdUser = await FindAsync<AppUser>(p => p.UserName == manager.UserName);
        AssertAssignedRolesAndClaimsInJwt(objResult.Value.Token, createdUser,
            [Roles.MaintenanceAdmin, Roles.Staff],
            [
                new(CustomClaimTypes.Id, user.Id.ToString(CultureInfo.InvariantCulture)),
                new(CustomClaimTypes.Username, user.UserName!),
            ]);
    }
}

[Collection(nameof(IntegrationTestBaseCollection))]
public class CreateTokenRefreshTokenTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_Unauthorized_when_invalid_refreshToken()
    {
        // Arrange
        var cmd = TestCreateToken(CreateToken.GrantType.RefreshToken).Generate();
        await EnrollCustomer(cmd.Username);

        // Act
        var result = await SendAsync(cmd);

        // Assert
        var unauthResult = AssertX.IsType<UnauthorizedHttpResult2<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(unauthResult.Value);
        Assert.Equal("Unable to refresh session.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Issues_jwt_and_returns_token()
    {
        // Arrange
        var cmd = TestCreateToken(CreateToken.GrantType.RefreshToken).Generate();
        var user = (await EnrollCustomer(cmd.Username)).User;

        // Act
        var result = await SendAsync(cmd with { Password = user.RefreshTokens.Single().Token });

        // Assert
        var objResult = AssertX.IsType<Ok<JwtResponse>>(result);
        var response = objResult.Value;
        Assert.Equal(cmd.Username, response.Username);
        Assert.NotEmpty(response.Token);
        user = (await FindAsync<Customer>(p => p.Username == cmd.Username, "User.RefreshTokens")).User;
        Assert.Equal(2, user.RefreshTokens.Count);
        Assert.Equal(user.RefreshTokens.Last().Token, response.RefreshToken);
        Assert.Equal(JwtDuration, Math.Round((response.TokenExpiry - DateTime.UtcNow).TotalMinutes));
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Handles_multiple_calls_and_concurrency()
    {
        // Arrange
        var cmd = TestCreateToken(CreateToken.GrantType.RefreshToken).Generate();
        var user = (await EnrollCustomer(cmd.Username)).User;
        var rt = user.RefreshTokens.Single().Token;

        var tasks = Enumerable.Range(1, 10).Select(p => SendAsync(cmd with { Password = rt }));

        // Act
        CreateToken.RefreshTokenTests = true;
        var results = await Task.WhenAll(tasks);
        CreateToken.RefreshTokenTests = false;

        // Assert
        user = (await FindAsync<Customer>(p => p.Username == cmd.Username, "User.RefreshTokens")).User;
        Assert.Equal(2, user.RefreshTokens.Count);

        foreach (var result in results)
        {
            var objResult = Assert.IsType<Ok<JwtResponse>>(result);
            var response = objResult.Value;
            Assert.Equal(cmd.Username, response.Username);
            Assert.NotEmpty(response.Token);
            Assert.Equal(user.RefreshTokens.Last().Token, response.RefreshToken);
            Assert.Equal(JwtDuration, Math.Round((response.TokenExpiry - DateTime.UtcNow).TotalMinutes));
        }
    }
}
