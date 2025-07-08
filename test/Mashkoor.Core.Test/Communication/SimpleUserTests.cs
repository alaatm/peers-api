using Mashkoor.Core.Communication;

namespace Mashkoor.Core.Test.Communication;

public class SimpleUserTests
{
    [Fact]
    public void Props_are_initialized()
    {
        // Arrange
        var user = new SimpleUser() { PreferredLanguage = "en", Email = null };

        // Act & Assert
        Assert.Equal("en", user.PreferredLanguage);
        Assert.Null(user.Email);
    }

    [Fact]
    public void UserHandles_sets_Handles_removing_null_values()
    {
        var user = new SimpleUser
        {
            UserHandles = ["test1", null, "test2", null],
        };

        Assert.Equal(["test1", "test2"], user.Handles);
    }
}
