using BuildingBlocks.CleanArchitecture.Domain.Output;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace BuildingBlocks.CleanArchitecture.Application.CQRS.Behaviors;

public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result

{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Processing request {RequestName}", requestName);

        TResponse result = await next();

        if (result.IsSuccess)
        {
            _logger.LogInformation("Request {RequestName} successfully completed", requestName);
        }
        else
        {
            using (LogContext.PushProperty("Error", result.Error, destructureObjects: true))
            {
                _logger.LogError("Request {RequestName} failed with error", requestName);
            }
        }

        return result;
    }
}
