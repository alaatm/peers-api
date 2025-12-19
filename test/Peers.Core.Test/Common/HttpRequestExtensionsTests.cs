using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Peers.Core.Common;

namespace Peers.Core.Test.Common;

public class HttpRequestExtensionsTests
{
    [Fact]
    public void GetQueryValue_returns_query_value_when_key_exists()
    {
        // Arrange
        var request = new DefaultHttpContext().Request;
        request.Query = new QueryCollection(new Dictionary<string, StringValues> { ["key"] = "value" });

        // Act
        var value = request.GetQueryValue("key");

        // Assert
        Assert.Equal("value", value);
    }

    [Fact]
    public void GetQueryValue_returns_null_when_key_does_not_exist()
    {
        // Arrange
        var request = new DefaultHttpContext().Request;

        // Act
        var value = request.GetQueryValue("key");

        // Assert
        Assert.Null(value);
    }
}
