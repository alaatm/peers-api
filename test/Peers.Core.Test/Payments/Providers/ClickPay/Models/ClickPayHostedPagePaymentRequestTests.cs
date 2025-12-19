using Peers.Core.Payments;
using Peers.Core.Payments.Providers.ClickPay.Models;

namespace Peers.Core.Test.Payments.Providers.ClickPay.Models;

public class ClickPayHostedPagePaymentRequestTests
{
    [Fact]
    public void Create_creates_correct_requestObject()
    {
        // Arrange
        var profileId = "testProfileId";
        var lang = "en";
        var returnUrl = new Uri("https://example.com/return");
        var callbackUrl = new Uri("https://example.com/callback");
        var info = PaymentInfo.ForHpp(12, Guid.NewGuid().ToString(), "test description", "1234567890", "info@example.com", new Dictionary<string, string>
        {
            { "k1", "v1" },
            { "k2", "v2" },
        });

        // Act
        var request = ClickPayHostedPagePaymentRequest.Create(profileId, lang, false, false, returnUrl, callbackUrl, info);

        // Assert
        Assert.Equal(profileId, request.ProfileId);
        Assert.Equal(info.OrderId, request.CartId);
        Assert.Equal(info.Description, request.CartDescription);
        Assert.Equal(info.Amount, request.CartAmount);
        Assert.Equal("SAR", request.CartCurrency);
        Assert.Equal(lang, request.Lang);
        Assert.Equal(returnUrl, request.ReturnUrl);
        Assert.Equal(callbackUrl, request.CallbackUrl);
        Assert.Equal(info.CustomerPhone, request.Customer.Phone);
        Assert.Equal(info.CustomerEmail, request.Customer.Email);
        Assert.Equal("Riyadh", request.Customer.Street1);
        Assert.Equal("Riyadh", request.Customer.City);
        Assert.Equal("SA", request.Customer.Country);
        Assert.Equal("12345", request.Customer.Zip);
        Assert.Null(request.Tokenize);
        Assert.True(request.HideShipping);
        Assert.True(request.ShowSaveCard);
        Assert.False(request.Framed);
        Assert.Equal("ecom", request.TranClass);
        Assert.Equal("sale", request.TranType);
        Assert.Equal("en", request.Lang);
        Assert.Same(info.Metadata, request.Metadata);
    }

    [Theory]
    [InlineData(true, "auth")]
    [InlineData(false, "sale")]
    public void Create_creates_correct_requestObject_depending_on_authOnly_value(bool authOnly, string expectedTranType)
    {
        // Arrange & act
        var request = ClickPayHostedPagePaymentRequest.Create(
            "profileId",
            "lang",
            authOnly,
            false,
            new Uri("https://example.com/return"),
            new Uri("https://example.com/callback"),
            PaymentInfo.ForHpp(12, Guid.NewGuid().ToString(), "test description", "1234567890", "info@example.com"));

        // Assert
        Assert.Equal(expectedTranType, request.TranType);
    }

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, null)]
    public void Create_creates_correct_requestObject_depending_on_tokenize_value(bool tokenize, int? expectedTokenize)
    {
        // Arrange & act
        var request = ClickPayHostedPagePaymentRequest.Create(
            "profileId",
            "lang",
            false,
            tokenize,
            new Uri("https://example.com/return"),
            new Uri("https://example.com/callback"),
            PaymentInfo.ForHpp(12, Guid.NewGuid().ToString(), "test description", "1234567890", "info@example.com"));

        // Assert
        Assert.Equal(expectedTokenize, request.Tokenize);
    }

    [Fact]
    public void Create_sets_cartId_to_orderId_value()
    {
        // Arrange
        var info = PaymentInfo.ForHpp(12, Guid.NewGuid().ToString(), "test description", "1234567890", "info@example.com");

        // Act
        var request = ClickPayHostedPagePaymentRequest.Create(
            "profileId",
            "lang",
            false,
            false,
            new Uri("https://example.com/return"),
            new Uri("https://example.com/callback"),
            info);

        // Assert
        Assert.Equal(info.OrderId, request.CartId);
    }
}
