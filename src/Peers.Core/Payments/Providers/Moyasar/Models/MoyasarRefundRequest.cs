using System.Text.Json.Serialization;
using Peers.Core.Common;

namespace Peers.Core.Payments.Providers.Moyasar.Models;

/// <summary>
/// Represents a payment refund request.
/// </summary>
public sealed class MoyasarRefundRequest
{
    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="MoyasarRefundRequest"/>.
    /// </summary>
    /// <param name="amount">The amount to capture.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static MoyasarRefundRequest Create(decimal amount)
    {
        if (amount.GetDecimalPlaces() > 2)
        {
            throw new ArgumentException("Amount must be in SAR and have a maximum of 2 decimal places.", nameof(amount));
        }

        return new()
        {
            Amount = (int)(amount * 100),
        };
    }
}
