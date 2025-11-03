using Peers.Core.Common;

namespace Peers.Core.Test.Common;

public class ListExtensionsTests
{
    [Fact]
    public void HasDuplicates_ShouldReturnTrueAndDuplicate_WhenDuplicatesExist()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 2 };
        // Act
        var result = list.HasDuplicates(out var duplicate);
        // Assert
        Assert.True(result);
        Assert.Equal(2, duplicate);
    }

    [Fact]
    public void HasDuplicates_ShouldReturnFalseAndDefault_WhenNoDuplicatesExist()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5 };
        // Act
        var result = list.HasDuplicates(out var duplicate);
        // Assert
        Assert.False(result);
        Assert.Equal(default, duplicate);
    }

    [Fact]
    public void HasDuplicates_ShouldHandleEmptyList()
    {
        // Arrange
        var list = new List<int>();
        // Act
        var result = list.HasDuplicates(out var duplicate);
        // Assert
        Assert.False(result);
        Assert.Equal(default, duplicate);
    }
}
