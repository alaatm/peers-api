using MediatR;
using Peers.Core.Commands;

namespace Peers.Core.Cqrs;

/// <summary>
/// Base contract for command handlers.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, IResult>
    where TCommand : ICommand
{
}
