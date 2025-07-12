using Mashkoor.Modules.Customers.Commands;
using Mashkoor.Modules.Customers.Domain;
using Mashkoor.Modules.Kernel.Startup;
using Mashkoor.Modules.Users.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Mashkoor.Modules.Test.Customers.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class ChangePinCodeTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_customer_role()
        => await AssertCommandAccess(new ChangePinCode.Command(default, default, default));

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_if_user_is_suspended()
    {
        // Arrange
        var customer = await EnrollCustomer(isSuspended: true);

        // Act
        var result = await SendAsync(new ChangePinCode.Command("000000", "111111", "111111"), customer);

        // Assert
        var badRequest = AssertX.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Your account is suspended.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_if_no_pin_exist()
    {
        // Arrange
        var customer = await EnrollCustomer();

        // Act
        var result = await SendAsync(new ChangePinCode.Command("000000", "111111", "111111"), customer);

        // Assert
        var badRequest = AssertX.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("You do not have a PIN code set.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_if_current_pinCode_is_wrong()
    {
        // Arrange
        var pinCode = "123456";
        var customer = await EnrollCustomer();
        AssertX.IsType<Created>(await SendAsync(new CreatePinCode.Command(pinCode, pinCode), customer));

        // Act
        var result = await SendAsync(new ChangePinCode.Command("000000", "111111", "111111"), customer);

        // Assert
        var badRequest = AssertX.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Current PIN code is incorrect.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_and_suspends_customer_after_5_failed_attempts()
    {
        // Arrange
        var pinCode = "123456";
        var customer = await EnrollCustomer();
        AssertX.IsType<Created>(await SendAsync(new CreatePinCode.Command(pinCode, pinCode), customer));
        await InsertManagerAsync(email: StartupBackgroundService.AdminUsername);

        // Act
        AssertWrongCurrentPinCodeResult(AssertX.IsType<BadRequest<ProblemDetails>>(await SendAsync(new ChangePinCode.Command("000000", "111111", "111111"), customer)));
        AssertWrongCurrentPinCodeResult(AssertX.IsType<BadRequest<ProblemDetails>>(await SendAsync(new ChangePinCode.Command("000000", "111111", "111111"), customer)));
        AssertWrongCurrentPinCodeResult(AssertX.IsType<BadRequest<ProblemDetails>>(await SendAsync(new ChangePinCode.Command("000000", "111111", "111111"), customer)));
        AssertWrongCurrentPinCodeResult(AssertX.IsType<BadRequest<ProblemDetails>>(await SendAsync(new ChangePinCode.Command("000000", "111111", "111111"), customer)));
        var result = AssertX.IsType<BadRequest<ProblemDetails>>(await SendAsync(new ChangePinCode.Command("000000", "111111", "111111"), customer));

        // Assert
        var badRequest = AssertX.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Your account has been suspended due to too many failed attempts.", problem.Detail);

        customer = Find<Customer>(customer.Id, "User");
        Assert.Equal(UserStatus.Suspended, customer.User.Status);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_NoContent_and_updates_customer_pinCode()
    {
        // Arrange
        var oldPinCode = "123456";
        var newPinCode = "654321";
        var customer = await EnrollCustomer();
        AssertX.IsType<Created>(await SendAsync(new CreatePinCode.Command(oldPinCode, oldPinCode), customer));

        // Act
        var result = await SendAsync(new ChangePinCode.Command(oldPinCode, newPinCode, newPinCode), customer);

        // Assert
        AssertX.IsType<NoContent>(result);
        customer = Find<Customer>(customer.Id, "User");
        Assert.NotNull(customer.PinCodeHash);
        ExecuteScope(services =>
        {
            var passwordHasher = services.GetRequiredService<IPasswordHasher<AppUser>>();
            Assert.Equal(PasswordVerificationResult.Success, passwordHasher.VerifyHashedPassword(customer.User, customer.PinCodeHash, newPinCode));
        });
    }

    private static void AssertWrongCurrentPinCodeResult(BadRequest<ProblemDetails> badRequest)
    {
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Current PIN code is incorrect.", problem.Detail);
    }
}
