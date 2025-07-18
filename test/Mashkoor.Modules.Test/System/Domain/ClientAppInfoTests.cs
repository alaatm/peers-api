using Mashkoor.Modules.System.Domain;

namespace Mashkoor.Modules.Test.System.Domain;

public class ClientAppInfoTests
{
    [Theory]
    [InlineData("android", "android")]
    [InlineData("ios", "ios")]
    [InlineData("?", null)]
    public void GetStoreLink_returns_storeLink_for_requested_platform(string platform, string expectedStoreLink)
    {
        // Arrange
        var clientAppInfo = new ClientAppInfo
        {
            AndroidStoreLink = "android",
            IOSStoreLink = "ios",
        };

        // Act
        var storeLink = clientAppInfo.GetStoreLink(platform);
        Assert.Equal(expectedStoreLink, storeLink);
    }
}
