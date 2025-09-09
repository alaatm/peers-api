using Peers.Modules.Users.Domain;

namespace Peers.Modules.Test.Users.Domain;

public class UserNotificationTests
{
    [Fact]
    public void Create_creates_instance()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var user = TestPwUser().Generate();
        var notification = Notification.Create(date, "test");

        // Act
        var userNotification = UserNotification.Create(user, notification);

        // Assert
        Assert.Same(user, userNotification.User);
        Assert.Same(notification, userNotification.Notification);
        Assert.Equal(date, notification.CreatedOn);
    }
}
