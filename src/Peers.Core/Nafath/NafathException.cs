using Peers.Core.Nafath.Models;

namespace Peers.Core.Nafath;

public sealed class NafathException : Exception
{
    /// <summary>
    /// The error response object.
    /// </summary>
    public NafathErrorResponse? ErrorObject { get; set; }

    public NafathException()
    {
    }

    public NafathException(string message) : base(message)
    {
    }

    public NafathException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public NafathException(string message, NafathErrorResponse error) : base(message)
        => ErrorObject = error;

    public override string ToString() => $"{base.ToString()}\n{ErrorObject}";
}
