using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using System.Globalization;

namespace Peers.Core.Payments.Providers.Moyasar.Models;

/// <summary>
/// Represents a Moyasar error response.
/// </summary>
public sealed class MoyasarErrorResponse
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;

    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;

    [JsonPropertyName("errors")]
    public object ErrorsEncoded { get; set; } = default!;

    [JsonIgnore]
    public Dictionary<string, object>? Errors => ErrorsEncoded is JsonElement je
        ? JsonElementToDictionary(je)
        : null;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"Type: {Type}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Message: {Message}");
        sb.AppendLine($"Errors:");
        sb.Append(ErrorsToString(Errors!));
        return sb.ToString();
    }

    private static Dictionary<string, object> JsonElementToDictionary(JsonElement element)
    {
        var dictionary = new Dictionary<string, object>();

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                dictionary.Add(property.Name, JsonElementToDictionary(property.Value));
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            var list = new List<object>();

            foreach (var item in element.EnumerateArray())
            {
                list.Add(JsonElementToDictionary(item));
            }

            dictionary.Add("array", list);
        }
        else
        {
            dictionary.Add("value", element.ToString());
        }

        return dictionary;
    }

    private static string ErrorsToString(Dictionary<string, object> errors)
    {
        var sb = new StringBuilder();

        foreach (var (key, value) in errors ?? [])
        {
            if (value is Dictionary<string, object> dict)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"{key}:");
                sb.Append(ErrorsToString(dict));
            }
            else if (value is List<object> list)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"{key}:");
                foreach (var item in list)
                {
                    sb.Append(ErrorsToString((Dictionary<string, object>)item));
                }
            }
            else
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"{key}: {value}");
            }
        }

        return sb.ToString();
    }
}
