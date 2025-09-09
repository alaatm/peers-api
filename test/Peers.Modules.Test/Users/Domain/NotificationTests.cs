using Peers.Modules.Users.Domain;

namespace Peers.Modules.Test.Users.Domain;

public class NotificationTests
{
    [Theory]
    [InlineData("user")]
    [InlineData(null)]
    public void Create_creates_instance(string createdBy)
    {
        // Arrange
        var date = DateTime.UtcNow;
        var contents = "test";

        // Act
        var notification = Notification.Create(date, contents, createdBy);

        // Assert
        Assert.Equal(date, notification.CreatedOn);
        Assert.Equal(createdBy is null ? "SYSTEM" : createdBy, notification.CreatedBy);
        Assert.Equal(contents, notification.Contents);
    }
}
