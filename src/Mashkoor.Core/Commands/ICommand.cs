using MediatR;

namespace Mashkoor.Core.Commands;

/// <summary>
/// Contract for any HTTP POST/PUT/PATCH command
/// </summary>
public interface ICommand : IRequest<IResult> { }
