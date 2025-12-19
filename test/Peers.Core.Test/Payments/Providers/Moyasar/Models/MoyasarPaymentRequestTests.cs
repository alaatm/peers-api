using Peers.Core.Payments;
using Peers.Core.Payments.Models;
using Peers.Core.Payments.Providers.Moyasar.Models;

namespace Peers.Core.Test.Payments.Providers.Moyasar.Models;

public class MoyasarPaymentRequestTests
{
    [Fact]
    public void Create_initializes_metadata_if_not_already_set()
    {
        // Arrange
        var amount = 1.23m;
        var token = Guid.NewGuid().ToString();
        var description = "description";
        var info = PaymentInfo.ForTransactionApi(amount, "orderId", description, metadata: null);

        var expectedMetadata = new Dictionary<string, string>()
        {
            { PaymentInfo.OrderIdKey, "orderId" },
        };

        // Act
        var paymentRequest = MoyasarPaymentRequest.Create(PaymentSourceType.TokenizedCard, true, token, info);

        // Assert
        Assert.NotNull(paymentRequest);
        Assert.Equal(expectedMetadata, paymentRequest.Metadata);
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
        var info = PaymentInfo.ForTransactionApi(amount, "orderId", description, metadata: metadata);

        var expectedMetadata = new Dictionary<string, string>()
        {
            { "k", "v" },
            { PaymentInfo.OrderIdKey, "orderId" },
        };

        // Act
        var paymentRequest = MoyasarPaymentRequest.Create(PaymentSourceType.ApplePay, immediateCapture, token, info);

        // Assert
        Assert.NotNull(paymentRequest);
        Assert.Equal(123, paymentRequest.Amount);
        Assert.Equal(description, paymentRequest.Description);
        Assert.Equal(expectedMetadata, paymentRequest.Metadata);

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
        var info = PaymentInfo.ForTransactionApi(amount, "orderId", description, metadata: metadata);

        var expectedMetadata = new Dictionary<string, string>()
        {
            { "k", "v" },
            { PaymentInfo.OrderIdKey, "orderId" },
        };

        // Act
        var paymentRequest = MoyasarPaymentRequest.Create(PaymentSourceType.TokenizedCard, immediateCapture, token, info);

        // Assert
        Assert.NotNull(paymentRequest);
        Assert.Equal(123, paymentRequest.Amount);
        Assert.Equal(description, paymentRequest.Description);
        Assert.Equal(expectedMetadata, paymentRequest.Metadata);

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
        var info = PaymentInfo.ForTransactionApi(amount, "orderId", description, metadata: metadata);

        // Act
        var exception = Record.Exception(() => MoyasarPaymentRequest.Create((PaymentSourceType)int.MaxValue, false, token, info));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<NotSupportedException>(exception);
        Assert.Equal("Payment type 2147483647 is not supported.", exception.Message);
    }
}
