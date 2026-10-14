using BonusScraper.Domain.Runs;
using ErrorOr;

namespace BonusScraper.Application.Abstractions;

/// <summary>Publishes a completed run's offers for the web page to read.</summary>
public interface IOfferPublisher
{
    /// <returns>Where the offers were published, for the run summary.</returns>
    Task<ErrorOr<string>> PublishAsync(ScrapeRun run, CancellationToken cancellationToken);
}
