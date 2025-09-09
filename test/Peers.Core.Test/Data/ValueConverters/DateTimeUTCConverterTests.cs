using Peers.Core.Data.ValueConverters;

namespace Peers.Core.Test.Data.ValueConverters;

public class DateTimeUTCConverterTests
{
    [Fact]
    public void DateTimeUTCConverter_converts_dateTime_to_dateTime_with_utc_kind()
    {
        // Arrange
        var testDate = DateTime.Now;
        var converter = new DateTimeUTCConverter();

        // Act
        var from = (DateTime)converter.ConvertFromProvider(testDate);
        var to = (DateTime)converter.ConvertToProvider(testDate);

        // Assert
        Assert.Equal(testDate.Ticks, from.Ticks);
        Assert.Equal(testDate.Ticks, to.Ticks);
        Assert.Equal(DateTimeKind.Utc, from.Kind);
        Assert.Equal(DateTimeKind.Local, to.Kind);
    }

    [Fact]
    public void NullableDateTimeUTCConverter_converts_dateTime_to_dateTime_with_utc_kind()
    {
        // Arrange
        DateTime? testDate = DateTime.Now;
        var converter = new NullableDateTimeUTCConverter();

        // Act
        var from = (DateTime?)converter.ConvertFromProvider(testDate);
        var to = (DateTime?)converter.ConvertToProvider(testDate);

        // Assert
        Assert.Equal(testDate.Value.Ticks, from.Value.Ticks);
        Assert.Equal(testDate.Value.Ticks, to.Value.Ticks);
        Assert.Equal(DateTimeKind.Utc, from.Value.Kind);
        Assert.Equal(DateTimeKind.Local, to.Value.Kind);
    }

    [Fact]
    public void NullableDateTimeUTCConverter_handles_null_DateTime()
    {
        // Arrange
        DateTime? testDate = null;
        var converter = new NullableDateTimeUTCConverter();

        // Act
        var from = (DateTime?)converter.ConvertFromProvider(testDate);
        var to = (DateTime?)converter.ConvertToProvider(testDate);

        // Assert
        Assert.Null(from);
        Assert.Null(to);
    }
}
