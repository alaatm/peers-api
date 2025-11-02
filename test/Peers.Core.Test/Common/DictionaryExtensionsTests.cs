using Peers.Core.Common;

namespace Peers.Core.Test.Common;

public class DictionaryExtensionsTests
{
    [Fact]
    public void GetOrAdd_ShouldAddValue_WhenKeyDoesNotExist()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>();
        var key = "testKey";
        static int valueFactory(string k) => 42;

        // Act
        var value = dictionary.GetOrAdd(key, valueFactory);

        // Assert
        Assert.Equal(42, value);
        Assert.True(dictionary.ContainsKey(key));
        Assert.Equal(42, dictionary[key]);
    }

    [Fact]
    public void GetOrAdd_ShouldReturnExistingValue_WhenKeyExists()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>
        {
            { "existingKey", 100 }
        };
        var key = "existingKey";
        static int valueFactory(string k) => 42;

        // Act
        var value = dictionary.GetOrAdd(key, valueFactory);

        // Assert
        Assert.Equal(100, value);
        Assert.Single(dictionary);
    }

    [Fact]
    public void GetOrAdd_ShouldCallFactoryOnlyOnce_PerKey()
    {
        // Arrange
        var dictionary = new Dictionary<string, int>();
        var key = "uniqueKey";
        var callCount = 0;
        int ValueFactory(string k)
        {
            callCount++;
            return 99;
        }

        // Act
        var value1 = dictionary.GetOrAdd(key, ValueFactory);
        var value2 = dictionary.GetOrAdd(key, ValueFactory);

        // Assert
        Assert.Equal(99, value1);
        Assert.Equal(99, value2);
        Assert.Equal(1, callCount);
    }
}
