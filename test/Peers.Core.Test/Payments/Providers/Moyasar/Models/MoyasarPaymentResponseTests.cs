using Peers.Core.Payments.Models;
using Peers.Core.Payments.Providers.Moyasar.Models;

namespace Peers.Core.Test.Payments.Providers.Moyasar.Models;

public class MoyasarPaymentResponseTests
{
    [Fact]
    public void ToGeneric_returns_correct_PaymentResponse_for_Payment()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse = new MoyasarPaymentResponse
        {
            Id = "123",
            Amount = 10000,
            Currency = "USD",
            CreatedAt = now,
            Status = MoyasarPaymentResponse.StatusPaid,
        };

        // Act
        var result = paymentResponse.ToGeneric();

        // Assert
        Assert.Equal("123", result.PaymentId);
        Assert.Equal(PaymentOperationType.Payment, result.Operation);
        Assert.Equal(100.00m, result.Amount);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(now, result.Timestamp);
        Assert.True(result.IsSuccessful);
        Assert.Same(paymentResponse, result.ProviderSpecificResponse);
    }

    [Fact]
    public void ToGeneric_returns_correct_PaymentResponse_for_Authorization()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse = new MoyasarPaymentResponse
        {
            Id = "123",
            Amount = 10000,
            Currency = "USD",
            CreatedAt = now,
            Status = MoyasarPaymentResponse.StatusAuth,
        };

        // Act
        var result = paymentResponse.ToGeneric();

        // Assert
        Assert.Equal("123", result.PaymentId);
        Assert.Equal(PaymentOperationType.Authorization, result.Operation);
        Assert.Equal(100.00m, result.Amount);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(now, result.Timestamp);
        Assert.True(result.IsSuccessful);
        Assert.Same(paymentResponse, result.ProviderSpecificResponse);
    }

    [Fact]
    public void ToGeneric_returns_correct_PaymentResponse_for_Capture()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse = new MoyasarPaymentResponse
        {
            Id = "123",
            Amount = 20000,
            Captured = 10000,
            Currency = "USD",
            CapturedAt = now,
            Status = MoyasarPaymentResponse.StatusCapture,
        };

        // Act
        var result = paymentResponse.ToGeneric();

        // Assert
        Assert.Equal("123", result.PaymentId);
        Assert.Equal(PaymentOperationType.Capture, result.Operation);
        Assert.Equal(100.00m, result.Amount);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(now, result.Timestamp);
        Assert.True(result.IsSuccessful);
        Assert.Same(paymentResponse, result.ProviderSpecificResponse);
    }

    [Fact]
    public void ToGeneric_returns_correct_PaymentResponse_for_Refund()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse = new MoyasarPaymentResponse
        {
            Id = "123",
            Amount = 20000,
            Captured = 15000,
            Refunded = 10000,
            Currency = "USD",
            RefundedAt = now,
            Status = MoyasarPaymentResponse.StatusRefund,
        };

        // Act
        var result = paymentResponse.ToGeneric();

        // Assert
        Assert.Equal("123", result.PaymentId);
        Assert.Equal(PaymentOperationType.Refund, result.Operation);
        Assert.Equal(100.00m, result.Amount);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(now, result.Timestamp);
        Assert.True(result.IsSuccessful);
        Assert.Same(paymentResponse, result.ProviderSpecificResponse);
    }

    [Fact]
    public void ToGeneric_returns_correct_PaymentResponse_for_Void()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse = new MoyasarPaymentResponse
        {
            Id = "123",
            Amount = 20000,
            Currency = "USD",
            VoidedAt = now,
            Status = MoyasarPaymentResponse.StatusVoid,
        };

        // Act
        var result = paymentResponse.ToGeneric();

        // Assert
        Assert.Equal("123", result.PaymentId);
        Assert.Equal(PaymentOperationType.Void, result.Operation);
        Assert.Equal(0, result.Amount);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(now, result.Timestamp);
        Assert.True(result.IsSuccessful);
        Assert.Same(paymentResponse, result.ProviderSpecificResponse);
    }

    [Fact]
    public void ToGeneric_returns_correct_PaymentResponse_for_Unknown()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse = new MoyasarPaymentResponse
        {
            Id = "123",
            Amount = 20000,
            Currency = "USD",
            Status = "?",
        };

        // Act
        var result = paymentResponse.ToGeneric();

        // Assert
        Assert.Equal("123", result.PaymentId);
        Assert.Equal(PaymentOperationType.Unknown, result.Operation);
        Assert.Equal(0, result.Amount);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(DateTime.MinValue, result.Timestamp);
        Assert.True(result.IsSuccessful);
        Assert.Same(paymentResponse, result.ProviderSpecificResponse);
    }

    [Fact]
    public void ToGeneric_sets_IsSuccessfull_to_false_when_status_is_failed()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse = new MoyasarPaymentResponse
        {
            Id = "123",
            Amount = 20000,
            Currency = "USD",
            Status = MoyasarPaymentResponse.StatusFailed,
        };

        // Act
        var result = paymentResponse.ToGeneric();

        // Assert
        Assert.False(result.IsSuccessful);
    }
}
