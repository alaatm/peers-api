using Mashkoor.Core.Security.Hashing;

namespace Mashkoor.Core.Test.Security.Hashing;

public class HmacHashTests
{
    [Fact]
    public void GenerateKey_ReturnsBase64EncodedKey()
    {
        // Arrange
        var hmacHash = new HmacHash();

        // Act
        var key = hmacHash.GenerateKey();

        // Assert
        Assert.NotNull(key);
        Assert.NotEmpty(key);

        // Ensure it's a valid url encoded Base64 string
        Assert.DoesNotContain(" ", key);
        Assert.DoesNotContain("=", key);
        Assert.DoesNotContain("+", key);
        Assert.DoesNotContain("/", key);
    }

    [Theory]
    [InlineData("z")]
    [InlineData("aaaaaaaaaaaaaa")]
    [InlineData("aaaaaaaaaaaaaz")]
    [InlineData("ca23c772e95f3f05a1a98198aad248c3b18f78caa8dff0bcd66f553421bedea61")]
    public void IsValidSignature_returns_false_for_bad_signature(string signature)
    {
        // Arrange
        var hmacHash = new HmacHash();
        var input = "Hello, world!";
        var key = "T_tAoFUCSSA0nIMCfWMSK8cAMOKJVbGyVKWSxz3zeiE";

        // Act
        var isValid = hmacHash.IsValidSignature(input, signature, key);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValidSignature_returns_true_for_valid_signature()
    {
        // Arrange
        var hmacHash = new HmacHash();
        var input = "Hello, world!";
        var signature = "ca23c772e95f3f05a1a98198aad248c3b18f78caa8dff0bcd66f553421bedea6";
        var key = "T_tAoFUCSSA0nIMCfWMSK8cAMOKJVbGyVKWSxz3zeiE";

        // Act
        var isValid = hmacHash.IsValidSignature(input, signature, key);

        // Assert
        Assert.True(isValid);
    }
}
