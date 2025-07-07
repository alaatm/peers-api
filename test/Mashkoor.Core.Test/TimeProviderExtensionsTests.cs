namespace Mashkoor.Core.Test;

public class TimeProviderExtensionsTests
{
    [Fact]
    public void UtcNow_returns_actual_utc_time()
        => Assert.InRange((TimeProvider.System.UtcNow() - DateTime.UtcNow).TotalSeconds, -1, 1);

    [Fact]
    public void UtcNow_returns_Utc_time()
    {
        // Arrange & act
        var now = TimeProvider.System.UtcNow();

        // Assert
        Assert.Equal(DateTimeKind.Utc, now.Kind);
    }

    [Fact]
    public void UtcToday_returns_correct_date_object()
    {
        // Arrange and act
        var today = TimeProvider.System.UtcToday();

        // Assert
        Assert.Equal(DateTime.UtcNow.Year, today.Year);
        Assert.Equal(DateTime.UtcNow.Month, today.Month);
        Assert.Equal(DateTime.UtcNow.Day, today.Day);
    }
}
