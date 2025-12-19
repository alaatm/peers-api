using System.Text.Json.Serialization;

namespace Peers.Core.Payments.Providers.Moyasar.Models;

/// <summary>
/// Represents a payment capture request.
/// </summary>
public sealed class MoyasarCaptureRequest
{
    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="MoyasarCaptureRequest"/>.
    /// </summary>
    /// <param name="amount">The amount to capture.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static MoyasarCaptureRequest Create(decimal amount) => new()
    {
        Amount = (int)(amount * 100),
    };
}
