using Peers.Core.Common;

namespace Peers.Core.Test.Common;

public class SlugHelperTests
{
    [Theory]
    [InlineData("Hello World!", "hello-world")]
    [InlineData("Hashtag# is weird.", "hashtag-is-weird")]
    [InlineData("  Leading and trailing spaces  ", "leading-and-trailing-spaces")]
    [InlineData("Multiple   spaces", "multiple-spaces")]
    [InlineData("Special & Characters / Test", "special-characters-test")]
    [InlineData("Accented éàü characters", "accented-eau-characters")]
    [InlineData("Mixed_SEPARATORS+and|symbols\\test", "mixed-separators-and-symbols-test")]
    [InlineData("----Multiple---Hyphens----", "multiple-hyphens")]
    public void ToSlug_ValidInput_ReturnsExpectedSlug(string input, string expected)
    {
        var slug = SlugHelper.ToSlug(input);
        Assert.Equal(expected, slug);
    }
}
