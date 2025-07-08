using Mashkoor.Core.Security.StrongKeys;

namespace Mashkoor.Core.Test.Security.StrongKeys;

public class KeyGeneratorTests
{
    [Theory]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(25)]
    public void Create_returns_key_of_requested_size(int size)
    {
        // Arrange & act
        var key = KeyGenerator.Create(size);

        // Assert
        Assert.Equal(size, key.Length);
        Assert.True(key.All(p => char.IsDigit(p) || char.IsLower(p) || char.IsUpper(p)));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(25)]
    public void Create_returns_digit_only_key_when_requested(int size)
    {
        // Arrange & act
        var key = KeyGenerator.Create(size, numbersOnly: true);

        // Assert
        Assert.Equal(size, key.Length);
        Assert.True(key.All(char.IsDigit));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(25)]
    public void Create_returns_digit_only_key_when_numbersOnlySet_and_lowerCaseOnly_is_set(int size)
    {
        // Arrange & act
        var key = KeyGenerator.Create(size, numbersOnly: true, lowerCaseOnly: true);

        // Assert
        Assert.Equal(size, key.Length);
        Assert.True(key.All(char.IsDigit));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(25)]
    public void Create_returns_lowerCase_letters_and_digit_when_lowerCaseOnly_is_set(int size)
    {
        // Arrange & act
        var key = KeyGenerator.Create(size, lowerCaseOnly: true);

        // Assert
        Assert.Equal(size, key.Length);
        Assert.True(key.All(p => char.IsDigit(p) || char.IsLower(p)));
    }
}
