using Peers.Modules.Customers.Domain;

namespace Peers.Modules.Test.Customers.Domain;

public class CustomerTests
{
    [Fact]
    public void Create_creates_customer()
    {
        // Arrange
        var user = Test2FUser().Generate();
        var secret = Guid.NewGuid().ToString("N");

        // Act
        var customer = Customer.Create(user, secret);

        // Assert
        Assert.NotNull(customer);
        Assert.Equal(user.UserName, customer.Username);
        Assert.Equal(secret, customer.Secret);
        Assert.Same(user, customer.User);
        Assert.Null(customer.PinCodeHash);
    }
}
