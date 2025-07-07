namespace Mashkoor.Core.Data.Identity;

/// <summary>
/// Represents an unsuccessful identity operation exception.
/// </summary>
public sealed class IdentityResultException : Exception
{
    public IdentityResultException(IEnumerable<string> errors)
        : base($"{Environment.NewLine}{string.Join(Environment.NewLine, errors)}") { }

    public IdentityResultException() => throw new NotImplementedException();
    public IdentityResultException(string message) : base(message) => throw new NotImplementedException();
    public IdentityResultException(string message, Exception innerException) => throw new NotImplementedException();
}
