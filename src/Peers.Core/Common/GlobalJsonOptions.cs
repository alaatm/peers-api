using System.Text.Json;

namespace Peers.Core.Common;

public static class GlobalJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
