namespace BonusScraper.Domain.Runs;

/// <summary>Why one casino produced no offer on a run. Codes and descriptions only: never page content.</summary>
public sealed record ScrapeFailure(string Casino, Uri Source, string Code, string Description);
