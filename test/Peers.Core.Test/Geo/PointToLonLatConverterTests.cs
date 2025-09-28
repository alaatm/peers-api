using System.Text.Json;
using NetTopologySuite.Geometries;
using Peers.Core.Geo;

namespace Peers.Core.Test.Geo;

public class PointToLonLatConverterTests
{
    [Theory]
    [InlineData(/*lang=json,strict*/ "{\"lat\": 40.7128}")]
    [InlineData(/*lang=json,strict*/ "{\"lon\": -74.0060}")]
    public void Deserialize_MissingFields_ThrowsException(string json)
    {
        // Arrange, act & assert
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Point>(json, GetOptions()));
        Assert.Contains("Point requires both 'lat' and 'lon' numeric fields", exception.Message);
    }

    [Fact]
    public void Deserialize_NonNumberFields_ThrowsException()
    {
        // Arrange
        var json = /*lang=json,strict*/ "{\"lat\": \"not-a-number\", \"lon\": -74.0060}";

        // Act & assert
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Point>(json, GetOptions()));
        Assert.Contains("Point requires both 'lat' and 'lon' numeric fields", exception.Message);
    }

    [Fact]
    public void Deserialize_UnknownFields_ThrowsException()
    {
        // Arrange
        var json = /*lang=json,strict*/ "{\"xxx\": \"xxx\"}";

        // Act & assert
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Point>(json, GetOptions()));
        Assert.Contains("Point requires both 'lat' and 'lon' numeric fields", exception.Message);
    }

    [Fact]
    public void Can_Deserialize_ValidJson()
    {
        // Arrange
        var json = /*lang=json,strict*/ "{\"lat\": 40.7128, \"lon\": -74.0060}";

        // Act
        var point = JsonSerializer.Deserialize<Point>(json, GetOptions());

        // Assert
        Assert.NotNull(point);
        Assert.Equal(-74.0060, point.X);
        Assert.Equal(40.7128, point.Y);
    }

    [Fact]
    public void Can_Serialize_Point()
    {
        // Arrange
        var point = GeometryHelper.CreatePoint(40.7128, -74.0060);

        // Act
        var json = JsonSerializer.Serialize(point, GetOptions());

        // Assert
        Assert.Equal(/*lang=json,strict*/ "{\"lat\":40.7128,\"lon\":-74.006}", json);
    }

    [Fact]
    public void Read_InvalidJson_ThrowsException()
    {
        // Arrange
        var reader = new Utf8JsonReader(new ReadOnlySpan<byte>([0x01, 0x02, 0x03]));
        var conv = new PointToLonLatConverter();

        // Act & assert
        try
        {
            conv.Read(ref reader, null, null);
            Assert.True(false);
        }
        catch (JsonException exception)
        {
            Assert.Contains("Expected object for Point", exception.Message);
            return;
        }
    }

    private static JsonSerializerOptions GetOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new PointToLonLatConverter());
        return options;
    }
}
