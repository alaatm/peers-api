using Peers.Core.Geo;

namespace Peers.Core.Test.Geo;

public class GeometryHelperTests
{
    [Fact]
    public void CreatePoint_creates_point()
    {
        // Arrange
        var lat = 38.8976;
        var lon = 77.0366;

        // Act
        var point = GeometryHelper.CreatePoint(lat, lon);

        // Assert
        Assert.Equal(lon, point.X);
        Assert.Equal(lat, point.Y);
        Assert.Equal(4326, point.SRID);
    }

    [Theory]
    [InlineData(38.8976, -77.0366, 39.9496, -75.1503, 199830)]
    [InlineData(24.660187716610512, 46.874895139591771, 24.660187716610512, 46.874895139591771, 0)] // Exact equal
    [InlineData(24.660187716610512, 46.874895139591759, 24.660187716610512, 46.874895139591771, 0)] // Extremly close
    public void DistanceBetween_returns_distance_between_two_points_in_meters(double lat1, double lon1, double lat2, double lon2, int expected)
    {
        // Arrange and act
        var distance = GeometryHelper.DistanceBetween(GeometryHelper.CreatePoint(lat1, lon1), GeometryHelper.CreatePoint(lat2, lon2));

        // Assert
        Assert.Equal(expected, (int)distance);
    }

    [Theory]
    [InlineData(24.716482, 45.334480, 24.690036, 45.389600, 6500, true)]
    [InlineData(24.716482, 45.334480, 24.690036, 45.389600, 6000, false)]
    public void IsWithinDistance_returns_whether_point1_is_within_specified_distance_in_meters_from_point2(double lat1, double lon1, double lat2, double lon2, double distance, bool expectedResult)
    {
        // Arrange and act
        var result = GeometryHelper.IsWithinDistance(GeometryHelper.CreatePoint(lat1, lon1), GeometryHelper.CreatePoint(lat2, lon2), distance);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void IsWithinDistance_handles_error_percentage_in_calc()
    {
        // Arrange
        var p1 = GeometryHelper.CreatePoint(26.421258506833386, 49.96410771086292);
        var p2 = GeometryHelper.CreatePoint(26.378890940759966, 49.981346714608975);
        Assert.True(GeometryHelper.DistanceBetween(p1.Y, p1.X, p2.Y, p2.X) > 5000);
        Assert.True(GeometryHelper.DistanceBetween(p1.Y, p1.X, p2.Y, p2.X) < 5020);

        // Act
        var result = GeometryHelper.IsWithinDistance(p1, p2, 5000);

        // Assert
        Assert.True(result);
    }
}
