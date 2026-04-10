using MediatR;
using Loop.SharedKernel;

namespace Loop.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;



