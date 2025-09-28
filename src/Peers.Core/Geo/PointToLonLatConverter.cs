using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;

namespace Peers.Core.Geo;

/// <summary>
/// Provides a custom JSON converter for serializing and deserializing <see cref="Point"/> objects using 'lat' and 'lon'
/// fields.
/// </summary>
public sealed class PointToLonLatConverter : JsonConverter<Point>
{
    private static ReadOnlySpan<byte> LatUtf8 => "lat"u8;
    private static ReadOnlySpan<byte> LonUtf8 => "lon"u8;

    /// <inheritdoc />
    public override Point? Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions __)
    {
        if (reader.TokenType is not JsonTokenType.StartObject)
        {
            throw new JsonException("Expected object for Point.");
        }

        double? lat = null, lon = null;

        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject)
        {
            if (reader.TokenType is JsonTokenType.PropertyName)
            {
                if (reader.ValueTextEquals(LatUtf8))
                {
                    lat = ReadValue(ref reader);
                }
                else if (reader.ValueTextEquals(LonUtf8))
                {
                    lon = ReadValue(ref reader);
                }
            }
        }

        if (lon is null || lat is null)
        {
            throw new JsonException("Point requires both 'lat' and 'lon' numeric fields.");
        }

        return GeometryHelper.CreatePoint(lat.Value, lon.Value);

        static double? ReadValue(ref Utf8JsonReader reader)
        {
            reader.Read();
            if (reader.TokenType is JsonTokenType.Number)
            {
                return reader.GetDouble();
            }
            else
            {
                reader.Skip();
            }

            return null;
        }
    }

    /// <inheritdoc />
    public override void Write([NotNull] Utf8JsonWriter writer, [NotNull] Point value, JsonSerializerOptions _)
    {
        writer.WriteStartObject();
        writer.WriteNumber(LatUtf8, value.Y);
        writer.WriteNumber(LonUtf8, value.X);
        writer.WriteEndObject();
    }
}
