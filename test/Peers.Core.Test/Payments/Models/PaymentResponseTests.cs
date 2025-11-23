using Peers.Core.Payments.Models;

namespace Peers.Core.Test.Payments.Models;

public class PaymentResponseTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Equals_ShouldReturnTrue_WhenAllPropertiesAreEqual(bool nullParent)
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse1 = TestPaymentResponse("123", nullParent ? null : "000", PaymentOperationType.Payment, 100.00m, "USD", now);
        var paymentResponse2 = TestPaymentResponse("123", nullParent ? null : "000", PaymentOperationType.Payment, 100.00m, "USD", now);

        // Act
        var result1 = paymentResponse1.Equals(paymentResponse2);
        var result2 = paymentResponse1.Equals((object)paymentResponse2);

        // Assert
        Assert.True(result1);
        Assert.True(result2);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenPaymentIdsAreDifferent()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse1 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now);
        var paymentResponse2 = TestPaymentResponse("456", "000", PaymentOperationType.Payment, 100.00m, "USD", now);

        // Act
        var result1 = paymentResponse1.Equals(paymentResponse2);
        var result2 = paymentResponse1.Equals((object)paymentResponse2);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenParentPaymentIdsAreDifferent()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse1 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now);
        var paymentResponse2 = TestPaymentResponse("123", "111", PaymentOperationType.Payment, 100.00m, "USD", now);

        // Act
        var result1 = paymentResponse1.Equals(paymentResponse2);
        var result2 = paymentResponse1.Equals((object)paymentResponse2);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenOperationsAreDifferent()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse1 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now);
        var paymentResponse2 = TestPaymentResponse("123", "000", PaymentOperationType.Refund, 100.00m, "USD", now);

        // Act
        var result1 = paymentResponse1.Equals(paymentResponse2);
        var result2 = paymentResponse1.Equals((object)paymentResponse2);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenAmountsAreDifferent()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse1 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now);
        var paymentResponse2 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 200.00m, "USD", now);

        // Act
        var result1 = paymentResponse1.Equals(paymentResponse2);
        var result2 = paymentResponse1.Equals((object)paymentResponse2);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenCurrenciesAreDifferent()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse1 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now);
        var paymentResponse2 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "EUR", now);

        // Act
        var result1 = paymentResponse1.Equals(paymentResponse2);
        var result2 = paymentResponse1.Equals((object)paymentResponse2);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenTimestampsAreDifferent()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse1 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now);
        var paymentResponse2 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now.AddMinutes(1));

        // Act
        var result1 = paymentResponse1.Equals(paymentResponse2);
        var result2 = paymentResponse1.Equals((object)paymentResponse2);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenOtherIsNull()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse1 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now);
        PaymentResponse typedNull = null;
        object objNull = null;

        // Act
        var result1 = paymentResponse1.Equals(typedNull);
        var result2 = paymentResponse1.Equals(objNull);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnSameValue_WhenAllPropertiesAreEqual()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse1 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now);
        var paymentResponse2 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now);

        // Act
        var hashCode1 = paymentResponse1.GetHashCode();
        var hashCode2 = paymentResponse2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnDifferentValues_WhenPaymentIdsAreDifferent()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse1 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now);
        var paymentResponse2 = TestPaymentResponse("456", "000", PaymentOperationType.Payment, 100.00m, "USD", now);

        // Act
        var hashCode1 = paymentResponse1.GetHashCode();
        var hashCode2 = paymentResponse2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnDifferentValues_WhenParentPaymentIdsAreDifferent()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse1 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now);
        var paymentResponse2 = TestPaymentResponse("123", "111", PaymentOperationType.Payment, 100.00m, "USD", now);

        // Act
        var hashCode1 = paymentResponse1.GetHashCode();
        var hashCode2 = paymentResponse2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnDifferentValues_WhenOperationsAreDifferent()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse1 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now);
        var paymentResponse2 = TestPaymentResponse("123", "000", PaymentOperationType.Refund, 100.00m, "USD", now);

        // Act
        var hashCode1 = paymentResponse1.GetHashCode();
        var hashCode2 = paymentResponse2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnDifferentValues_WhenAmountsAreDifferent()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse1 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now);
        var paymentResponse2 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 200.00m, "USD", now);

        // Act
        var hashCode1 = paymentResponse1.GetHashCode();
        var hashCode2 = paymentResponse2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnDifferentValues_WhenCurrenciesAreDifferent()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse1 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now);
        var paymentResponse2 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "EUR", now);

        // Act
        var hashCode1 = paymentResponse1.GetHashCode();
        var hashCode2 = paymentResponse2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnDifferentValues_WhenTimestampsAreDifferent()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var paymentResponse1 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now);
        var paymentResponse2 = TestPaymentResponse("123", "000", PaymentOperationType.Payment, 100.00m, "USD", now.AddMinutes(1));

        // Act
        var hashCode1 = paymentResponse1.GetHashCode();
        var hashCode2 = paymentResponse2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    private static PaymentResponse TestPaymentResponse(
        string paymentId,
        string parentPaymentId,
        PaymentOperationType operation,
        decimal amount,
        string currency,
        DateTime timestamp) => new()
        {
            PaymentId = paymentId,
            ParentPaymentId = parentPaymentId,
            Operation = operation,
            Amount = amount,
            Currency = currency,
            Timestamp = timestamp,
        };
}
