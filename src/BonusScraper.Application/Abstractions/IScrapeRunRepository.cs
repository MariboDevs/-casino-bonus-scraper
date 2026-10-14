using BonusScraper.Domain.Runs;
using ErrorOr;

namespace BonusScraper.Application.Abstractions;

/// <summary>Durable history of runs and the offers each produced.</summary>
public interface IScrapeRunRepository
{
    Task<ErrorOr<Success>> SaveAsync(ScrapeRun run, CancellationToken cancellationToken);
}
