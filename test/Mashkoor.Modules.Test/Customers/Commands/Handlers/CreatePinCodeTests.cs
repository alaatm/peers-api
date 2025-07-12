using Mashkoor.Modules.Customers.Commands;
using Mashkoor.Modules.Customers.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Mashkoor.Modules.Test.Customers.Commands.Handlers;

[Collection(nameof(IntegrationTestBaseCollection))]
public class CreatePinCodeTests : IntegrationTestBase
{
    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Requires_customer_role()
        => await AssertCommandAccess(new CreatePinCode.Command(default, default));

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_BadRequest_if_pin_already_exist()
    {
        // Arrange
        var pinCode = "123456";
        var customer = await EnrollCustomer();
        AssertX.IsType<Created>(await SendAsync(new CreatePinCode.Command(pinCode, pinCode), customer));

        // Act
        var result = await SendAsync(new CreatePinCode.Command(pinCode, pinCode), customer);

        // Assert
        var badRequest = AssertX.IsType<BadRequest<ProblemDetails>>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("PIN code already exists for this account.", problem.Detail);
    }

    [SkippableFact(typeof(PlatformNotSupportedException))]
    public async Task Returns_created_and_stores_pinCodeHash()
    {
        // Arrange
        var pinCode = "123456";
        var customer = await EnrollCustomer();

        // Act
        var result = await SendAsync(new CreatePinCode.Command(pinCode, pinCode), customer);

        // Assert
        AssertX.IsType<Created>(result);
        customer = Find<Customer>(customer.Id);
        Assert.NotNull(customer.PinCodeHash);
    }
}
