using Peers.Core.Payments.Providers.ClickPay.Models;

namespace Peers.Core.Test.Payments.Providers.ClickPay.Models;

public class ClickPayTransactionRequestTests
{
    [Fact]
    public void CreateSale_creates_sale_request()
    {
        // Arrange
        var profileId = "test_profile_id";
        var amount = 100.00m;
        var token = "test_token";
        var description = "test_description";
        var metadata = new Dictionary<string, string> { { "booking", "b123" } };

        // Act
        var request = ClickPayTransactionRequest.CreateSale(profileId, amount, token, description, metadata);

        // Assert
        Assert.Equal(profileId, request.ProfileId);
        Assert.Equal("sale", request.TransactionType);
        Assert.Equal("recurring", request.TransactionClass);
        Assert.Equal("b123", request.CartId);
        Assert.Equal(description, request.Description);
        Assert.Equal(metadata, request.Metadata);
        Assert.Equal(amount, request.Amount);
        Assert.Equal("SAR", request.Currency);
        Assert.Equal(token, request.Token);
    }

    [Fact]
    public void CreateAuthorization_creates_authorization_request()
    {
        // Arrange
        var profileId = "test_profile_id";
        var amount = 100.00m;
        var token = "test_token";
        var description = "test_description";
        var metadata = new Dictionary<string, string> { { "booking", "b123" } };

        // Act
        var request = ClickPayTransactionRequest.CreateAuthorization(profileId, amount, token, description, metadata);

        // Assert
        Assert.Equal(profileId, request.ProfileId);
        Assert.Equal("auth", request.TransactionType);
        Assert.Equal("recurring", request.TransactionClass);
        Assert.Equal("b123", request.CartId);
        Assert.Equal(description, request.Description);
        Assert.Equal(metadata, request.Metadata);
        Assert.Equal(amount, request.Amount);
        Assert.Equal("SAR", request.Currency);
        Assert.Equal(token, request.Token);
    }

    [Fact]
    public void CreateCapture_creates_capture_request()
    {
        // Arrange
        var profileId = "test_profile_id";
        var paymentId = "test_payment_id";
        var amount = 100.00m;
        var description = "test_description";
        var metadata = new Dictionary<string, string> { { "booking", "b123" } };

        // Act
        var request = ClickPayTransactionRequest.CreateCapture(profileId, paymentId, amount, description, metadata);

        // Assert
        Assert.Equal(profileId, request.ProfileId);
        Assert.Equal("capture", request.TransactionType);
        Assert.Equal("ecom", request.TransactionClass);
        Assert.Equal("b123", request.CartId);
        Assert.Equal(description, request.Description);
        Assert.Equal(metadata, request.Metadata);
        Assert.Equal(amount, request.Amount);
        Assert.Equal("SAR", request.Currency);
    }

    [Fact]
    public void CreateCapture_creates_capture_request_with_null_metadata()
    {
        // Arrange
        var profileId = "test_profile_id";
        var paymentId = "test_payment_id";
        var amount = 100.00m;
        var description = "test_description";

        // Act
        var request = ClickPayTransactionRequest.CreateCapture(profileId, paymentId, amount, description, null);

        // Assert
        Assert.Matches(@"^[{(]?[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}[)}]?$", request.CartId);
        Assert.Null(request.Metadata);
    }

    [Fact]
    public void CreateVoid_creates_void_request()
    {
        // Arrange
        var profileId = "test_profile_id";
        var paymentId = "test_payment_id";
        var amount = 100.00m;
        var description = "test_description";
        var metadata = new Dictionary<string, string> { { "booking", "b123" } };

        // Act
        var request = ClickPayTransactionRequest.CreateVoid(profileId, paymentId, amount, description, metadata);

        // Assert
        Assert.Equal(profileId, request.ProfileId);
        Assert.Equal("void", request.TransactionType);
        Assert.Equal("ecom", request.TransactionClass);
        Assert.Equal("b123", request.CartId);
        Assert.Equal(description, request.Description);
        Assert.Equal(metadata, request.Metadata);
        Assert.Equal(amount, request.Amount);
        Assert.Equal("SAR", request.Currency);
    }

    [Fact]
    public void CreateVoid_creates_void_request_with_null_metadata()
    {
        // Arrange
        var profileId = "test_profile_id";
        var paymentId = "test_payment_id";
        var amount = 100.00m;
        var description = "test_description";

        // Act
        var request = ClickPayTransactionRequest.CreateVoid(profileId, paymentId, amount, description, null);

        // Assert
        Assert.Matches(@"^[{(]?[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}[)}]?$", request.CartId);
        Assert.Null(request.Metadata);
    }

    [Fact]
    public void CreateRefund_creates_refund_request()
    {
        // Arrange
        var profileId = "test_profile_id";
        var paymentId = "test_payment_id";
        var amount = 100.00m;
        var description = "test_description";
        var metadata = new Dictionary<string, string> { { "booking", "b123" } };

        // Act
        var request = ClickPayTransactionRequest.CreateRefund(profileId, paymentId, amount, description, metadata);

        // Assert
        Assert.Equal(profileId, request.ProfileId);
        Assert.Equal("refund", request.TransactionType);
        Assert.Equal("ecom", request.TransactionClass);
        Assert.Equal("b123", request.CartId);
        Assert.Equal(description, request.Description);
        Assert.Equal(metadata, request.Metadata);
        Assert.Equal(amount, request.Amount);
        Assert.Equal("SAR", request.Currency);
    }

    [Fact]
    public void CreateRefund_creates_refund_request_with_null_metadata()
    {
        // Arrange
        var profileId = "test_profile_id";
        var paymentId = "test_payment_id";
        var amount = 100.00m;
        var description = "test_description";
        // Act
        var request = ClickPayTransactionRequest.CreateRefund(profileId, paymentId, amount, description, null);
        // Assert
        Assert.Matches(@"^[{(]?[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}[)}]?$", request.CartId);
        Assert.Null(request.Metadata);
    }

    [Fact]
    public void CreateQuery_creates_query_request()
    {
        // Arrange
        var profileId = "test_profile_id";
        var paymentId = "test_payment_id";

        // Act
        var request = ClickPayTransactionRequest.CreateQuery(profileId, paymentId);

        // Assert
        Assert.Equal(profileId, request.ProfileId);
        Assert.Equal(paymentId, request.TransactionRef);
    }

    [Fact]
    public void CreateTokenQueryOrDelete_creates_query_request()
    {
        // Arrange
        var profileId = "test_profile_id";
        var token = "test_token";

        // Act
        var request = ClickPayTransactionRequest.CreateTokenQueryOrDelete(profileId, token);

        // Assert
        Assert.Equal(profileId, request.ProfileId);
        Assert.Equal(token, request.Token);
    }
}
