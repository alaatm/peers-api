namespace Peers.Core.Payments;

/// <summary>
/// Represents payment provider exception.
/// </summary>
public class PaymentProviderException : Exception
{
    public PaymentProviderException()
    {
    }

    public PaymentProviderException(string message) : base(message)
    {
    }

    public PaymentProviderException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
