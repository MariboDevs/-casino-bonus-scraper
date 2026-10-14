using System.Diagnostics;
using ErrorOr;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BonusScraper.Application.Behaviours;

/// <summary>Logs the message type, duration and error codes. Never the message or response content.</summary>
public sealed partial class LoggingBehaviour<TMessage, TResponse>(ILogger<LoggingBehaviour<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
    where TResponse : IErrorOr
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var name = typeof(TMessage).Name;
        var started = Stopwatch.GetTimestamp();

        var response = await next(message, cancellationToken);
        var elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;

        if (response.IsError)
        {
            LogFailed(name, elapsed, string.Join(", ", response.Errors!.Select(e => e.Code)));
        }
        else
        {
            LogHandled(name, elapsed);
        }

        return response;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{Message} handled in {ElapsedMs:0} ms")]
    private partial void LogHandled(string message, double elapsedMs);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Message} failed in {ElapsedMs:0} ms: {ErrorCodes}")]
    private partial void LogFailed(string message, double elapsedMs, string errorCodes);
}
