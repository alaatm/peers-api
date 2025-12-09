using Peers.Core.Payments.Models;
using Peers.Core.Payments.Providers.ClickPay.Models;

namespace Peers.Core.Test.Payments.Providers.ClickPay.Models;

public class ClickPayPaymentResponseTests
{
    [Theory]
    [InlineData("A", true)]
    [InlineData("a", false)]
    [InlineData("E", false)]
    public void ToGeneric_sets_IsSuccessful_based_on_ResponseStatus(string responseStatus, bool expectedIsSuccessful)
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse = new ClickPayPaymentResponse
        {
            TranRef = "123",
            TranType = ClickPayPaymentResponse.StatusPaid,
            CartAmount = "100.50",
            CartCurrency = "USD",
            PaymentResult = new ClickPayPaymentResult { TransactionTime = now, ResponseStatus = responseStatus },
        };

        // Act
        var result = paymentResponse.ToGeneric();

        // Assert
        Assert.Equal(expectedIsSuccessful, result.IsSuccessful);
    }

    [Fact]
    public void ToGeneric_returns_correct_PaymentResponse_for_Payment()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse = new ClickPayPaymentResponse
        {
            TranRef = "123",
            TranType = ClickPayPaymentResponse.StatusPaid,
            CartAmount = "100.50",
            CartCurrency = "USD",
            PaymentResult = new ClickPayPaymentResult { TransactionTime = now },
        };

        // Act
        var result = paymentResponse.ToGeneric();

        // Assert
        Assert.Equal("123", result.PaymentId);
        Assert.Equal(PaymentOperationType.Payment, result.Operation);
        Assert.Equal(100.50m, result.Amount);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(now, result.Timestamp);
        Assert.Same(paymentResponse, result.ProviderSpecificResponse);
    }

    [Fact]
    public void ToGeneric_returns_correct_PaymentResponse_for_Authorization()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse = new ClickPayPaymentResponse
        {
            TranRef = "123",
            TranType = ClickPayPaymentResponse.StatusAuth,
            CartAmount = "100.50",
            CartCurrency = "USD",
            PaymentResult = new ClickPayPaymentResult { TransactionTime = now },
        };

        // Act
        var result = paymentResponse.ToGeneric();

        // Assert
        Assert.Equal("123", result.PaymentId);
        Assert.Equal(PaymentOperationType.Authorization, result.Operation);
        Assert.Equal(100.50m, result.Amount);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(now, result.Timestamp);
        Assert.Same(paymentResponse, result.ProviderSpecificResponse);
    }

    [Fact]
    public void ToGeneric_returns_correct_PaymentResponse_for_Capture()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse = new ClickPayPaymentResponse
        {
            TranRef = "123",
            TranType = ClickPayPaymentResponse.StatusCapture,
            CartAmount = "100.50",
            CartCurrency = "USD",
            PaymentResult = new ClickPayPaymentResult { TransactionTime = now },
        };

        // Act
        var result = paymentResponse.ToGeneric();

        // Assert
        Assert.Equal("123", result.PaymentId);
        Assert.Equal(PaymentOperationType.Capture, result.Operation);
        Assert.Equal(100.50m, result.Amount);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(now, result.Timestamp);
        Assert.Same(paymentResponse, result.ProviderSpecificResponse);
    }

    [Fact]
    public void ToGeneric_returns_correct_PaymentResponse_for_Refund()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse = new ClickPayPaymentResponse
        {
            TranRef = "123",
            TranType = ClickPayPaymentResponse.StatusRefund,
            CartAmount = "100.50",
            CartCurrency = "USD",
            PaymentResult = new ClickPayPaymentResult { TransactionTime = now },
        };

        // Act
        var result = paymentResponse.ToGeneric();

        // Assert
        Assert.Equal("123", result.PaymentId);
        Assert.Equal(PaymentOperationType.Refund, result.Operation);
        Assert.Equal(100.50m, result.Amount);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(now, result.Timestamp);
        Assert.Same(paymentResponse, result.ProviderSpecificResponse);
    }

    [Fact]
    public void ToGeneric_returns_correct_PaymentResponse_for_Void()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse = new ClickPayPaymentResponse
        {
            TranRef = "123",
            TranType = ClickPayPaymentResponse.StatusVoid,
            CartAmount = "100.50",
            CartCurrency = "USD",
            PaymentResult = new ClickPayPaymentResult { TransactionTime = now },
        };

        // Act
        var result = paymentResponse.ToGeneric();

        // Assert
        Assert.Equal("123", result.PaymentId);
        Assert.Equal(PaymentOperationType.Void, result.Operation);
        Assert.Equal(100.50m, result.Amount);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(now, result.Timestamp);
        Assert.Same(paymentResponse, result.ProviderSpecificResponse);
    }

    [Fact]
    public void ToGeneric_returns_correct_PaymentResponse_for_Unknown()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse = new ClickPayPaymentResponse
        {
            TranRef = "123",
            TranType = "?",
            CartAmount = "100.50",
            CartCurrency = "USD",
            PaymentResult = new ClickPayPaymentResult { TransactionTime = now },
        };

        // Act
        var result = paymentResponse.ToGeneric();

        // Assert
        Assert.Equal("123", result.PaymentId);
        Assert.Equal(PaymentOperationType.Unknown, result.Operation);
        Assert.Equal(100.50m, result.Amount);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(now, result.Timestamp);
        Assert.Same(paymentResponse, result.ProviderSpecificResponse);
    }
}
