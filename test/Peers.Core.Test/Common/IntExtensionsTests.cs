using Peers.Core.Common;

namespace Peers.Core.Test.Common;

public class IntExtensionsTests
{
    [Theory]
    [InlineData(0, "000000")]
    [InlineData(10, "00000A")]
    [InlineData(35, "00000Z")]
    [InlineData(36, "000010")]
    [InlineData(123456, "002N9C")]
    [InlineData(int.MaxValue, "ZIK0ZJ")]
    public void EncodeBase36_returns_base36_string(int value, string expected)
    {
        // Arrange & Act
        var actual = value.EncodeBase36();

        // Assert
        Assert.Equal(expected, actual);
    }
}
