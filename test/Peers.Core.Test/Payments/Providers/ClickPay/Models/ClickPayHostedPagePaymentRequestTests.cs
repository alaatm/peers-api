using Peers.Core.Payments.Providers.ClickPay.Models;

namespace Peers.Core.Test.Payments.Providers.ClickPay.Models;

public class ClickPayHostedPagePaymentRequestTests
{
    [Fact]
    public void Create_creates_correct_requestObject()
    {
        // Arrange
        var profileId = "testProfileId";
        var amount = 12;
        var customerPhone = "1234567890";
        var customerEmail = "info@example.com";
        var lang = "en";
        var returnUrl = new Uri("https://example.com/return");
        var callbackUrl = new Uri("https://example.com/callback");
        var description = "test description";
        var metadata = new Dictionary<string, string>
        {
            { "k1", "v1" },
            { "k2", "v2" },
        };

        // Act
        var request = ClickPayHostedPagePaymentRequest.Create(profileId, description, lang, amount, false, false, customerPhone, customerEmail, returnUrl, callbackUrl, metadata);

        // Assert
        Assert.Equal(profileId, request.ProfileId);
        Assert.Equal(description, request.CartDescription);
        Assert.Equal(amount, request.CartAmount);
        Assert.Equal("SAR", request.CartCurrency);
        Assert.Equal(lang, request.Lang);
        Assert.Equal(returnUrl, request.ReturnUrl);
        Assert.Equal(callbackUrl, request.CallbackUrl);
        Assert.Equal(customerPhone, request.Customer.Phone);
        Assert.Equal(customerEmail, request.Customer.Email);
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
        Assert.Same(metadata, request.Metadata);
    }

    [Theory]
    [InlineData(true, "auth")]
    [InlineData(false, "sale")]
    public void Create_creates_correct_requestObject_depending_on_authOnly_value(bool authOnly, string expectedTranType)
    {
        // Arrange & act
        var request = ClickPayHostedPagePaymentRequest.Create(
            "profileId",
            "description",
            "lang",
            10,
            authOnly,
            false,
            "customerPhone",
            "customerEmail",
            new Uri("https://example.com/return"),
            new Uri("https://example.com/callback"),
            []);

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
            "description",
            "lang",
            10,
            false,
            tokenize,
            "customerPhone",
            "customerEmail",
            new Uri("https://example.com/return"),
            new Uri("https://example.com/callback"),
            []);

        // Assert
        Assert.Equal(expectedTokenize, request.Tokenize);
    }

    [Fact]
    public void Create_sets_cartId_to_booking_metadata_value_if_set()
    {
        // Arrange & act
        var request = ClickPayHostedPagePaymentRequest.Create(
            "profileId",
            "description",
            "lang",
            10,
            false,
            false,
            "customerPhone",
            "customerEmail",
            new Uri("https://example.com/return"),
            new Uri("https://example.com/callback"),
            new Dictionary<string, string>
            {
                { "booking", "b123" },
                { "customer", "c123" },
            });

        // Assert
        Assert.Equal("b123", request.CartId);
    }

    [Fact]
    public void Create_sets_cartId_to_customer_metadata_value_if_set()
    {
        // Arrange & act
        var request = ClickPayHostedPagePaymentRequest.Create(
            "profileId",
            "description",
            "lang",
            10,
            false,
            false,
            "customerPhone",
            "customerEmail",
            new Uri("https://example.com/return"),
            new Uri("https://example.com/callback"),
            new Dictionary<string, string>
            {
                { "customer", "c123" },
            });

        // Assert
        Assert.Equal("c123", request.CartId);
    }

    [Fact]
    public void Create_sets_cartId_to_randomGuid_when_no_booking_or_customer_metadata_value_is_set()
    {
        // Arrange & act
        var request = ClickPayHostedPagePaymentRequest.Create(
            "profileId",
            "description",
            "lang",
            10,
            false,
            false,
            "customerPhone",
            "customerEmail",
            new Uri("https://example.com/return"),
            new Uri("https://example.com/callback"),
            new Dictionary<string, string>
            {
                { "x", "y" },
            });

        // Assert
        Assert.Matches(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", request.CartId);
    }
}
