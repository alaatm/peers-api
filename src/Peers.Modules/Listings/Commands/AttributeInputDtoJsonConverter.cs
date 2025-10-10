using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Peers.Modules.Listings.Commands.SetAttributes.Command;

namespace Peers.Modules.Listings.Commands;

public sealed class AttributeInputDtoJsonConverter : JsonConverter<AttributeInputDto>
{
    private const string DateFormat = "yyyy-MM-dd";

    public override AttributeInputDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions _) => reader.TokenType switch
    {
        JsonTokenType.Number => new AttributeInputDto.Numeric(ReadDecimal(ref reader)),
        JsonTokenType.True => new AttributeInputDto.Bool(true),
        JsonTokenType.False => new AttributeInputDto.Bool(false),
        JsonTokenType.String => ReadStringOrDateOnly(ref reader),
        JsonTokenType.StartArray => ReadArray(ref reader),
        _ => throw new JsonException($"Unsupported JSON token: '{reader.TokenType}' at position {reader.TokenStartIndex}."),
    };

    public override void Write(
        [NotNull] Utf8JsonWriter writer,
        [NotNull] AttributeInputDto value,
        JsonSerializerOptions _)
    {
        switch (value)
        {
            case AttributeInputDto.Numeric(var d):
                writer.WriteNumberValue(d);
                return;

            case AttributeInputDto.Bool(var b):
                writer.WriteBooleanValue(b);
                return;

            case AttributeInputDto.Date(var dt):
                writer.WriteStringValue(dt.ToString(DateFormat, CultureInfo.InvariantCulture));
                return;

            case AttributeInputDto.OptionCodeOrScalarString(var s):
                writer.WriteStringValue(s);
                return;

            case AttributeInputDto.OptionCodeAxis(var codes):
                WriteStringArray(writer, codes);
                return;

            case AttributeInputDto.NumericAxis(var nums):
                WriteDecimalArray(writer, nums);
                return;

            case AttributeInputDto.GroupAxis(var matrix):
                writer.WriteStartArray();
                foreach (var row in matrix)
                {
                    WriteDecimalArray(writer, row.Value);
                }
                writer.WriteEndArray();
                return;

            default:
                throw new JsonException($"Unknown {nameof(AttributeInputDto)} subtype: {value.GetType().Name}.");
        }
    }

    private static AttributeInputDto ReadStringOrDateOnly(ref Utf8JsonReader reader)
    {
        var s = reader.GetString()!;
        if (DateOnly.TryParseExact(s, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return new AttributeInputDto.Date(date);
        }
        return new AttributeInputDto.OptionCodeOrScalarString(s);
    }

    private static AttributeInputDto ReadArray(ref Utf8JsonReader reader)
    {
        // currently on StartArray
        if (!reader.Read())
        {
            throw new JsonException($"Unexpected end of JSON inside array at position {reader.TokenStartIndex}.");
        }

        if (reader.TokenType == JsonTokenType.EndArray)
        {
            throw new JsonException($"Empty arrays are not supported at position {reader.TokenStartIndex}.");
        }

        return reader.TokenType switch
        {
            JsonTokenType.String => new AttributeInputDto.OptionCodeAxis(ReadStringArray(ref reader)),
            JsonTokenType.Number => new AttributeInputDto.NumericAxis(ReadDecimalArray(ref reader)),
            JsonTokenType.StartArray => new AttributeInputDto.GroupAxis(ReadDecimalMatrix(ref reader)),
            _ => throw new JsonException($"Unsupported array element kind: {reader.TokenType} at position {reader.TokenStartIndex}."),
        };
    }

    private static List<string> ReadStringArray(ref Utf8JsonReader reader)
    {
        var list = new List<string>();
        while (true)
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Expected all elements to be 'String', but found '{reader.TokenType}' at position {reader.TokenStartIndex}.");
            }

            list.Add(reader.GetString()!);

            if (!reader.Read())
            {
                throw new JsonException($"Unexpected end of JSON in array at position {reader.TokenStartIndex}.");
            }
        }

        return list;
    }

    private static List<decimal> ReadDecimalArray(ref Utf8JsonReader reader)
    {
        var list = new List<decimal>();
        while (true)
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException($"Expected all elements to be 'Number', but found '{reader.TokenType}' at position {reader.TokenStartIndex}.");
            }

            list.Add(ReadDecimal(ref reader));

            if (!reader.Read())
            {
                throw new JsonException($"Unexpected end of JSON in array at position {reader.TokenStartIndex}.");
            }
        }

        return list;
    }

    private static List<AttributeInputDto.NumericAxis> ReadDecimalMatrix(ref Utf8JsonReader reader)
    {
        var rows = new List<AttributeInputDto.NumericAxis>();

        while (true)
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException($"GroupAxis must be an array of arrays of numbers, but found '{reader.TokenType}' at position {reader.TokenStartIndex}.");
            }

            // enter row
            if (!reader.Read())
            {
                throw new JsonException($"Unexpected end of JSON entering GroupAxis row at position {reader.TokenStartIndex}.");
            }

            if (reader.TokenType == JsonTokenType.EndArray)
            {
                throw new JsonException($"GroupAxis rows must not be empty at position {reader.TokenStartIndex}.");
            }

            rows.Add(new(ReadDecimalArray(ref reader)));

            // move past the row's EndArray
            if (!reader.Read())
            {
                throw new JsonException($"Unexpected end of JSON after GroupAxis row at position {reader.TokenStartIndex}.");
            }
        }

        if (rows.Count == 0)
        {
            throw new JsonException("GroupAxis must not be an empty array.");
        }

        return rows;
    }

    private static decimal ReadDecimal(ref Utf8JsonReader reader)
    {
        if (!reader.TryGetDecimal(out var d))
        {
            throw new JsonException($"Number at position {reader.TokenStartIndex} cannot be represented as decimal.");
        }

        return d;
    }

    private static void WriteStringArray(Utf8JsonWriter writer, List<string> arr)
    {
        writer.WriteStartArray();
        foreach (var s in arr)
        {
            writer.WriteStringValue(s);
        }

        writer.WriteEndArray();
    }

    private static void WriteDecimalArray(Utf8JsonWriter writer, List<decimal> arr)
    {
        writer.WriteStartArray();
        foreach (var n in arr)
        {
            writer.WriteNumberValue(n);
        }

        writer.WriteEndArray();
    }
}
