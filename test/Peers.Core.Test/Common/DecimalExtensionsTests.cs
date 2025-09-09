using Peers.Core.Common;

namespace Peers.Core.Test.Common;

public class DecimalExtensionsTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(-1, 0)]
    [InlineData(0.1, 1)]
    [InlineData(0.01, 2)]
    [InlineData(0.001, 3)]
    [InlineData(0.0001, 4)]
    [InlineData(0.00001, 5)]
    [InlineData(0.000001, 6)]
    [InlineData(0.0000001, 7)]
    [InlineData(-0.1, 1)]
    [InlineData(-0.01, 2)]
    [InlineData(-0.001, 3)]
    [InlineData(-0.0001, 4)]
    [InlineData(-0.00001, 5)]
    [InlineData(-0.000001, 6)]
    [InlineData(-0.0000001, 7)]
    public void GetDecimalPlaces_returns_decimal_places_count(decimal value, int expected)
    {
        // Arrange & Act
        var actual = value.GetDecimalPlaces();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetDecimalPlaces_returns_correct_value_in_special_conditions()
    {
        // Arrange
        var value = (decimal)-0.01f;

        // Act
        var decimalPlaces = value.GetDecimalPlaces();

        // Assert
        Assert.Equal(2, decimalPlaces);
    }
}
