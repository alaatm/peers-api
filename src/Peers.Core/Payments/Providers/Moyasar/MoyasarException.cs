using Peers.Core.Payments.Providers.Moyasar.Models;

namespace Peers.Core.Payments.Providers.Moyasar;

/// <summary>
/// Represents Moyasar payment provider exception.
/// </summary>
public class MoyasarException : PaymentProviderException
{
    /// <summary>
    /// The error response object.
    /// </summary>
    public MoyasarErrorResponse ErrorObject { get; set; } = default!;

    public MoyasarException()
    {
    }

    public MoyasarException(string message) : base(message)
    {
    }

    public MoyasarException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public MoyasarException(string message, MoyasarErrorResponse error) : base(message)
        => ErrorObject = error;

    public override string ToString() => $"{base.ToString()}\n{ErrorObject}";
}
