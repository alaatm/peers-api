using System.Diagnostics.CodeAnalysis;

namespace Peers.Core.Domain.Errors;

public sealed class DomainException : Exception
{
    public DomainError Error { get; }

    public DomainException([NotNull] DomainError error) : base(error.Code) => Error = error;

    [ExcludeFromCodeCoverage] public DomainException() => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public DomainException(string message) : base(message) => throw new NotImplementedException();
    [ExcludeFromCodeCoverage] public DomainException(string message, Exception innerException) : base(message, innerException) => throw new NotImplementedException();
}
