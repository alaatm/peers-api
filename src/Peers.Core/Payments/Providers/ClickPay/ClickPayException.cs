using Peers.Core.Payments.Providers.ClickPay.Models;

namespace Peers.Core.Payments.Providers.ClickPay;

/// <summary>
/// Represents ClickPay payment provider exception.
/// </summary>
public class ClickPayException : PaymentProviderException
{
    /// <summary>
    /// The error response object.
    /// </summary>
    public ClickPayErrorResponse ErrorObject { get; set; } = default!;

    public ClickPayException()
    {
    }

    public ClickPayException(string message) : base(message)
    {
    }

    public ClickPayException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ClickPayException(string message, ClickPayErrorResponse error) : base(message)
        => ErrorObject = error;

    public override string ToString() => $"{base.ToString()}\n{ErrorObject}";
}
