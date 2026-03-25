using Application.Abstractions.Messaging;
using MediatR;
using SharedKernel;

namespace Infrastructure.Messaging;

internal sealed class Dispatcher(ISender sender) : IDispatcher
{
    public Task<Result> Dispatch(ICommand command, CancellationToken cancellationToken = default) =>
        sender.Send(command, cancellationToken);

    public Task<Result<TResponse>> Dispatch<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default) =>
        sender.Send(command, cancellationToken);

    public Task<Result<TResponse>> Dispatch<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default) =>
        sender.Send(query, cancellationToken);
}
