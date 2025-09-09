using Peers.Core.Communication;
using Peers.Modules.Users;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Test.Users;

public class AppUserQuadrableExtensionsTests
{
    [Fact]
    public void ProjectToSimpleUser_returns_simpleUser_objects()
    {
        // Arrange
        var users = new[] { Test2FUser().Generate(), TestPwUser().Generate() };
        users[0].DeviceList.Add(TestDevice(users[0]).Generate());
        users[1].DeviceList = [TestDevice(users[0]).Generate()];

        // Act
        var r = users.AsQueryable().ProjectToSimpleUser().ToArray();

        // Assert
        Assert.Equal(2, r.Length);
        AssertUser(users[0], r[0]);
        AssertUser(users[1], r[1]);

        Assert.Equal(2, r[0].Handles.Count());
        Assert.Equal(2, users[0].DeviceList.Count);

        Assert.Single(r[1].Handles);
        Assert.Single(users[1].DeviceList);
    }

    [Fact]
    public void ToSimpleUser_returns_simpleUser_object_mfa()
    {
        // Arrange
        var user = Test2FUser().Generate();

        // Act
        var simpleUser = user.ToSimpleUser();

        // Assert
        AssertUser(user, simpleUser);

        Assert.Single(simpleUser.Handles);
        Assert.Single(user.DeviceList);
    }

    [Fact]
    public void ToSimpleUser_returns_simpleUser_object_pwd()
    {
        // Arrange
        var user = TestPwUser().Generate();
        user.DeviceList = [TestDevice(user).Generate()];

        // Act
        var simpleUser = user.ToSimpleUser();

        // Assert
        AssertUser(user, simpleUser);

        Assert.Single(simpleUser.Handles);
        Assert.Single(user.DeviceList);
    }

    private static void AssertUser(AppUser user, SimpleUser simpleUser)
    {
        Assert.Equal(user.PreferredLanguage, simpleUser.PreferredLanguage);
        Assert.Equal(user.DeviceList.Count, simpleUser.Handles.Count());
        Assert.Equal(user.Email, simpleUser.Email);

        var activeDevices = user.DeviceList.ToArray();
        for (var i = 0; i < activeDevices.Length; i++)
        {
            var device = activeDevices[i];
            Assert.Equal(device.PnsHandle, simpleUser.Handles.ElementAt(i));
        }
    }
}
