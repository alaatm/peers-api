using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Peers.Core.Domain.Errors;

public interface IDebuggable
{
    [ExcludeFromCodeCoverage]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    string D { get; }
}
