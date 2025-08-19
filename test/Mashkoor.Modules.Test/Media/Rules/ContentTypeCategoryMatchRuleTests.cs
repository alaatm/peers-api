using Mashkoor.Modules.Media.Domain;
using Mashkoor.Modules.Media.Rules;

namespace Mashkoor.Modules.Test.Media.Rules;

public class ContentTypeCategoryMatchRuleTests : DomainEntityTestBase
{
    [Fact]
    public void Sets_correct_title()
    {
        // Arrange and act
        var errorTitle = new ContentTypeCategoryMatchRule(default, default).ErrorTitle;

        // Assert
        Assert.Equal("Error adding media", errorTitle);
    }

    [Fact]
    public void Reports_error_when_content_type_does_not_match_category()
    {
        // Arrange
        var rule = new ContentTypeCategoryMatchRule("image/jpeg", MediaCategory.Video);

        // Act
        var result = rule.IsBroken();

        // Assert
        Assert.True(result);
        Assert.Equal("Content type 'image/jpeg' does not match category 'Video'", Assert.Single(rule.Errors));
    }

    [Fact]
    public void Passes_when_content_type_matches_category()
    {
        // Arrange
        var rule = new ContentTypeCategoryMatchRule("image/jpeg", MediaCategory.Image);

        // Act
        var result = rule.IsBroken();

        // Assert
        Assert.False(result);
        Assert.Empty(rule.Errors);
    }
}
