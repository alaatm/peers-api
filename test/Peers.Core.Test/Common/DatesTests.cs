using Peers.Core.Common;

namespace Peers.Core.Test.Common;

public class DatesTests
{
    [Theory]
    [MemberData(nameof(ToDateOnly_returns_correct_date_object_TestData))]
    public void ToDateOnly_returns_correct_date_object(DateTime? dateTime, DateOnly? expected)
    {
        // Arrange and act
        var date = dateTime.ToDateOnly();

        // Assert
        Assert.Equal(expected, date);
    }

    public static TheoryData<DateTime?, DateOnly?> ToDateOnly_returns_correct_date_object_TestData() => new()
    {
        { null, null },
        { new DateTime(2010, 5, 5, 5, 5, 5), new DateOnly(2010, 5, 5) },
    };

    [Fact]
    public void ToSaudiTimeZone_returns_saudi_time()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;

        // Act
        var saudiTime = utcNow.ToSaudiTimeZone();

        // Assert
        Assert.Equal(utcNow.AddHours(3), saudiTime);
    }

    [Fact]
    public void ToSaudiTimeZoneOffset_returns_saudi_time()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;

        // Act
        var saudiTime = utcNow.ToSaudiTimeZoneOffset();

        // Assert
        Assert.Equal(utcNow.AddHours(3), saudiTime.DateTime);
        Assert.Equal(TimeSpan.FromHours(3), saudiTime.Offset);
    }
}
