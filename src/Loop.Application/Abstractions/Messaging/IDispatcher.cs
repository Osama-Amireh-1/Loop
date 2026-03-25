using SharedKernel;

namespace Application.Abstractions.Messaging;

public interface IDispatcher
{
    Task<Result> Dispatch(ICommand command, CancellationToken cancellationToken = default);
    Task<Result<TResponse>> Dispatch<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
    Task<Result<TResponse>> Dispatch<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
