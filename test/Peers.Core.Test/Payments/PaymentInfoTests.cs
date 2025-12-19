using Peers.Core.Payments;

namespace Peers.Core.Test.Payments;

public class PaymentInfoTests
{
    #region ForTokenization
    [Fact]
    public void ForTokenization_creates_instance_with_expected_values()
    {
        // Arrange
        var customerId = 5;
        var customerPhone = "+966511111111";
        var customerEmail = "info@example.com";

        // Act
        var paymentInfo = PaymentInfo.ForTokenization(customerId, customerPhone, customerEmail);

        // Assert
        Assert.Equal(PaymentInfoIntent.Tokenization, paymentInfo.Intent);
        Assert.Equal(1m, paymentInfo.Amount);
        Assert.Equal($"{customerId}", paymentInfo.OrderId);
        Assert.Equal("Card Tokenization", paymentInfo.Description);
        Assert.Equal(customerPhone, paymentInfo.CustomerPhone);
        Assert.Equal(customerEmail, paymentInfo.CustomerEmail);
        Assert.Null(paymentInfo.Metadata);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ForTokenization_throws_when_null_or_empty_customerPhone(string customerPhone)
    {
        var exType = customerPhone is null ? typeof(ArgumentNullException) : typeof(ArgumentException);
        var ex = Assert.Throws(exType, () => PaymentInfo.ForTokenization(1, customerPhone, "test@email.com"));
        Assert.Contains("customerPhone", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ForTokenization_throws_when_null_or_empty_customerEmail(string customerEmail)
    {
        var exType = customerEmail is null ? typeof(ArgumentNullException) : typeof(ArgumentException);
        var ex = Assert.Throws(exType, () => PaymentInfo.ForTokenization(1, "+966511111111", customerEmail));
        Assert.Contains("customerEmail", ex.Message);
    }
    #endregion

    #region ForHpp
    [Fact]
    public void ForHpp_creates_instance_with_expected_values()
    {
        // Arrange
        var amount = 150.75m;
        var orderId = Guid.NewGuid().ToString();
        var description = "Test Payment";
        var customerPhone = "+966511111111";
        var customerEmail = "info@example.com";
        var metadata = new Dictionary<string, string>
        {
            { "Key1", "Value1" },
            { "Key2", "Value2" }
        };

        // Act
        var paymentInfo = PaymentInfo.ForHpp(amount, orderId, description, customerPhone, customerEmail, metadata);

        // Assert
        Assert.Equal(PaymentInfoIntent.HostedPaymentPage, paymentInfo.Intent);
        Assert.Equal(amount, paymentInfo.Amount);
        Assert.Equal(orderId, paymentInfo.OrderId);
        Assert.Equal(description, paymentInfo.Description);
        Assert.Equal(customerPhone, paymentInfo.CustomerPhone);
        Assert.Equal(customerEmail, paymentInfo.CustomerEmail);
        Assert.Equal(metadata, paymentInfo.Metadata);
    }

    [Fact]
    public void ForHpp_throws_when_amount_has_more_than_two_decimal_places()
    {
        var ex = Assert.Throws<ArgumentException>(() => PaymentInfo.ForHpp(1.234m, "orderId", "description", "1234567890", "test@email.com"));
        Assert.Equal("amount", ex.ParamName);
        Assert.Equal("Amount must be in SAR and have a maximum of 2 decimal places. (Parameter 'amount')", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ForHpp_throws_when_null_or_empty_orderId(string orderId)
    {
        var exType = orderId is null ? typeof(ArgumentNullException) : typeof(ArgumentException);
        var ex = Assert.Throws(exType, () => PaymentInfo.ForHpp(100m, orderId, "description", "1234567890", "test@email.com"));
        Assert.Contains("orderId", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ForHpp_throws_when_null_or_empty_description(string description)
    {
        var exType = description is null ? typeof(ArgumentNullException) : typeof(ArgumentException);
        var ex = Assert.Throws(exType, () => PaymentInfo.ForHpp(100m, "orderId", description, "1234567890", "test@email.com"));
        Assert.Contains("description", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ForHpp_throws_when_null_or_empty_customerPhone(string customerPhone)
    {
        var exType = customerPhone is null ? typeof(ArgumentNullException) : typeof(ArgumentException);
        var ex = Assert.Throws(exType, () => PaymentInfo.ForHpp(100m, "orderId", "description", customerPhone, "test@email.com"));
        Assert.Contains("customerPhone", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ForHpp_throws_when_null_or_empty_customerEmail(string customerEmail)
    {
        var exType = customerEmail is null ? typeof(ArgumentNullException) : typeof(ArgumentException);
        var ex = Assert.Throws(exType, () => PaymentInfo.ForHpp(100m, "orderId", "description", "+966511111111", customerEmail));
        Assert.Contains("customerEmail", ex.Message);
    }
    #endregion

    #region ForTransactionApi
    [Fact]
    public void ForTransactionApi_creates_instance_with_expected_values()
    {
        // Arrange
        var amount = 150.75m;
        var orderId = Guid.NewGuid().ToString();
        var description = "Test Payment";
        var metadata = new Dictionary<string, string>
        {
            { "Key1", "Value1" },
            { "Key2", "Value2" }
        };

        // Act
        var paymentInfo = PaymentInfo.ForTransactionApi(amount, orderId, description, metadata);

        // Assert
        Assert.Equal(PaymentInfoIntent.TransactionApi, paymentInfo.Intent);
        Assert.Equal(amount, paymentInfo.Amount);
        Assert.Equal(orderId, paymentInfo.OrderId);
        Assert.Equal(description, paymentInfo.Description);
        Assert.Null(paymentInfo.CustomerPhone);
        Assert.Null(paymentInfo.CustomerEmail);
        Assert.Equal(metadata, paymentInfo.Metadata);
    }

    [Fact]
    public void ForTransactionApi_throws_when_amount_has_more_than_two_decimal_places()
    {
        var ex = Assert.Throws<ArgumentException>(() => PaymentInfo.ForTransactionApi(1.234m, "orderId", "description"));
        Assert.Equal("amount", ex.ParamName);
        Assert.Equal("Amount must be in SAR and have a maximum of 2 decimal places. (Parameter 'amount')", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ForTransactionApi_throws_when_null_or_empty_orderId(string orderId)
    {
        var exType = orderId is null ? typeof(ArgumentNullException) : typeof(ArgumentException);
        var ex = Assert.Throws(exType, () => PaymentInfo.ForTransactionApi(100m, orderId, "description"));
        Assert.Contains("orderId", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ForTransactionApi_throws_when_null_or_empty_description(string description)
    {
        var exType = description is null ? typeof(ArgumentNullException) : typeof(ArgumentException);
        var ex = Assert.Throws(exType, () => PaymentInfo.ForTransactionApi(100m, "orderId", description));
        Assert.Contains("description", ex.Message);
    }
    #endregion

    [Fact]
    public void PromoteToHppIntent_throws_when_intent_is_not_tokenization()
    {
        var pi = PaymentInfo.ForHpp(100m, "orderId", "description", "+966511111111", "test@email.com");
        var ex = Assert.Throws<InvalidOperationException>(pi.PromoteToHppIntent);
        Assert.Contains("Only payments with Tokenization intent can be promoted to HPP intent.", ex.Message);

        pi = PaymentInfo.ForTransactionApi(100m, "orderId", "description");
        ex = Assert.Throws<InvalidOperationException>(pi.PromoteToHppIntent);
        Assert.Contains("Only payments with Tokenization intent can be promoted to HPP intent.", ex.Message);
    }

    [Fact]
    public void PromoteToHppIntent_promotes_intent_from_tokenization_to_hpp()
    {
        // Arrange
        var paymentInfo = PaymentInfo.ForTokenization(1, "+966511111111", "test@email.com");
        // Act
        paymentInfo.PromoteToHppIntent();
        // Assert
        Assert.Equal(PaymentInfoIntent.HostedPaymentPage, paymentInfo.Intent);
    }
}
