using MediatR;
using Loop.SharedKernel;

namespace Loop.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;


