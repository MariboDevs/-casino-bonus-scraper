using BonusScraper.Application.Scraping;
using BonusScraper.Domain.Runs;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BonusScraper.Cli;

/// <summary>Runs one scrape and turns its result into console output and an exit code.</summary>
public sealed partial class ScrapeCommandRunner(IMediator mediator, IOptions<ScrapeOptions> options, ILogger<ScrapeCommandRunner> logger)
{
    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ScrapeCasinosCommand(options.Value.ToTargets()), cancellationToken);

        if (result.IsError)
        {
            foreach (var error in result.Errors)
            {
                LogError(error.Code, error.Description);
            }

            return ExitCodes.Failed;
        }

        var summary = result.Value;
        LogSummary(summary.RunId, summary.Offers, summary.Provisional, summary.Failures.Count, summary.PublishedTo);
        foreach (var failure in summary.Failures)
        {
            LogFailure(failure.Casino, failure.Code, failure.Description);
        }

        return summary.Outcome == ScrapeRunOutcome.Succeeded ? ExitCodes.Success : ExitCodes.PartiallySucceeded;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Scrape failed: {ErrorCode} {Description}")]
    private partial void LogError(string errorCode, string description);

    [LoggerMessage(Level = LogLevel.Information, Message = "Run {RunId}: {Offers} offers ({Provisional} provisional), {Failures} failures, published to {PublishedTo}")]
    private partial void LogSummary(Guid runId, int offers, int provisional, int failures, string publishedTo);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Casino} failed: {ErrorCode} {Description}")]
    private partial void LogFailure(string casino, string errorCode, string description);
}
