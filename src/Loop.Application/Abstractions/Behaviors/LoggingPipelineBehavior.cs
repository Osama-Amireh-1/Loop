using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Abstractions.Behaviors;

public sealed class LoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            string requestName = typeof(TRequest).Name;
            logger.LogInformation("Processing request {RequestName}", requestName);
        }

        TResponse response = await next(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            string requestName = typeof(TRequest).Name;
            logger.LogInformation("Completed request {RequestName}", requestName);
        }

        return response;
    }
}
