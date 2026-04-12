using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using MediatR;

namespace Loop.Application.Abstractions.Behaviors;

public sealed class UnitOfWorkPipelineBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only commit for commands, not queries
        if (!IsCommand())
        {
            return await next(cancellationToken);
        }

        TResponse response = await next(cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }

    private static bool IsCommand()
    {
        return typeof(TRequest).GetInterfaces().Any(i =>
            i == typeof(ICommand) ||
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }
}
