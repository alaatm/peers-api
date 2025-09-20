namespace Peers.Core.Domain.Errors;

public sealed record DomainError(
    string? TitleCode,
    string Code,
    params object[] Args)
{
    public DomainError(string code, params object[] args)
        : this(null, code, args)
    {
    }
}
