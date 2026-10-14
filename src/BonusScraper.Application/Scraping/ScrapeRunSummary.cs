using BonusScraper.Domain.Runs;

namespace BonusScraper.Application.Scraping;

/// <summary>Counts and identifiers only, so it is safe to log in full.</summary>
public sealed record ScrapeRunSummary(
    Guid RunId,
    ScrapeRunOutcome Outcome,
    int Offers,
    int Provisional,
    IReadOnlyList<ScrapeFailure> Failures,
    string PublishedTo);
