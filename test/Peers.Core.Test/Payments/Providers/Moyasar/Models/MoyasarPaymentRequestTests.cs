using Peers.Core.Payments.Models;
using Peers.Core.Payments.Providers.Moyasar.Models;

namespace Peers.Core.Test.Payments.Providers.Moyasar.Models;

public class MoyasarPaymentRequestTests
{
    [Fact]
    public void Create_with_amount_with_more_than_two_decimal_places__throws_ArgumentException()
    {
        // Arrange
        var amount = 1.234m;

        // Act
        var exception = Record.Exception(() => MoyasarPaymentRequest.Create(default, amount, default, default, default, default));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
        Assert.Equal("Amount must be in SAR and have a maximum of 2 decimal places. (Parameter 'amount')", exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_can_create_applePay_PaymentRequest(bool immediateCapture)
    {
        // Arrange
        var amount = 1.23m;
        var token = Guid.NewGuid().ToString();
        var description = "description";
        var metadata = new Dictionary<string, string>() { { "k", "v" } };

        // Act
        var paymentRequest = MoyasarPaymentRequest.Create(PaymentSourceType.ApplePay, amount, immediateCapture, token, description, metadata);

        // Assert
        Assert.NotNull(paymentRequest);
        Assert.Equal(123, paymentRequest.Amount);
        Assert.Equal(description, paymentRequest.Description);
        Assert.Equal(metadata, paymentRequest.Metadata);

        var source = Assert.IsType<MoyasarApplePayPaymentSource>(paymentRequest.Source);
        Assert.Equal(immediateCapture ? "false" : "true", source.Manual);
        Assert.Equal(token, source.Token);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_can_create_tokenized_PaymentRequest(bool immediateCapture)
    {
        // Arrange
        var amount = 1.23m;
        var token = Guid.NewGuid().ToString();
        var description = "description";
        var metadata = new Dictionary<string, string>() { { "k", "v" } };

        // Act
        var paymentRequest = MoyasarPaymentRequest.Create(PaymentSourceType.TokenizedCard, amount, immediateCapture, token, description, metadata);

        // Assert
        Assert.NotNull(paymentRequest);
        Assert.Equal(123, paymentRequest.Amount);
        Assert.Equal(description, paymentRequest.Description);
        Assert.Equal(metadata, paymentRequest.Metadata);

        var source = Assert.IsType<MoyasarTokenPaymentSource>(paymentRequest.Source);
        Assert.Equal(immediateCapture ? "false" : "true", source.Manual);
        Assert.Equal(token, source.Token);
        Assert.False(source.ThreeDSecure);
    }

    [Fact]
    public void Create_with_unsupported_PaymentSourceType_throws_NotSupportedException()
    {
        // Arrange
        var amount = 1.23m;
        var token = Guid.NewGuid().ToString();
        var description = "description";
        var metadata = new Dictionary<string, string>() { { "k", "v" } };

        // Act
        var exception = Record.Exception(() => MoyasarPaymentRequest.Create((PaymentSourceType)int.MaxValue, amount, false, token, description, metadata));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<NotSupportedException>(exception);
        Assert.Equal("Payment type 2147483647 is not supported.", exception.Message);
    }
}
