using Peers.Modules.Users.Domain;

namespace Peers.Modules.Test.Users.Domain;

public class RefreshTokenTests
{
    [Theory]
    [MemberData(nameof(IsActive_tests_data))]
    public void IsActive_tests(DateTime? revokeDate, bool isActive)
    {
        // Arrange
        var rt = new RefreshToken() { Revoked = revokeDate };

        // Act
        var result = rt.IsActive;

        // Assert
        Assert.Equal(isActive, result);
    }

    public static TheoryData<DateTime?, bool> IsActive_tests_data => new()
    {
        { DateTime.UtcNow, false },
        { null, true },
    };

    [Fact]
    public void Create_creates_token()
    {
        // Arrange and act
        var date = DateTime.UtcNow;
        var rt = RefreshToken.Create(date);

        // Assert
        Assert.NotNull(rt);
        Assert.Equal(date, rt.Created);
        Assert.NotEmpty(rt.Token);
        Assert.Null(rt.Revoked);
    }
}
