using System.Diagnostics.CodeAnalysis;

namespace Peers.Core.Domain.Errors;

public sealed class InvalidDomainStateException : Exception
{
    [ExcludeFromCodeCoverage] public InvalidDomainStateException(string message) : base(message) { }
    [ExcludeFromCodeCoverage] public InvalidDomainStateException([NotNull] IDebuggable source, string message) : this($"[{source.D}]:\n{message}") { }

    [ExcludeFromCodeCoverage] public InvalidDomainStateException(string message, Exception innerException) : base(message, innerException) { }
    [ExcludeFromCodeCoverage] public InvalidDomainStateException([NotNull] IDebuggable source, string message, Exception innerException) : this($"[{source.D}]:\n{message}", innerException) { }

    [ExcludeFromCodeCoverage] public InvalidDomainStateException() => throw new NotImplementedException();
}
