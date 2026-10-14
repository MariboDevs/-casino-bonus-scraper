using BonusScraper.Application.Abstractions;
using BonusScraper.Domain.Runs;
using ErrorOr;

namespace BonusScraper.Infrastructure.Persistence;

/// <summary>Used when no database is configured, so the one-command run needs nothing but .NET.</summary>
internal sealed class NullScrapeRunRepository : IScrapeRunRepository
{
    public Task<ErrorOr<Success>> SaveAsync(ScrapeRun run, CancellationToken cancellationToken) =>
        Task.FromResult<ErrorOr<Success>>(Result.Success);
}
