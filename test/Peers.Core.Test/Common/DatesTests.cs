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
    public void ToSaudiTimeZone_returns_uae_time()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;

        // Act
        var uaeTime = utcNow.ToUaeTimeZone();

        // Assert
        Assert.Equal(utcNow.AddHours(4), uaeTime);
    }

    [Fact]
    public void ToSaudiTimeZoneOffset_returns_uae_time()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;

        // Act
        var uaeTime = utcNow.ToUaeTimeZoneOffset();

        // Assert
        Assert.Equal(utcNow.AddHours(4), uaeTime.DateTime);
        Assert.Equal(TimeSpan.FromHours(4), uaeTime.Offset);
    }
}
