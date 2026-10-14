using System.ComponentModel.DataAnnotations;
using BonusScraper.Application.Scraping;

namespace BonusScraper.Cli;

public sealed class ScrapeOptions
{
    public const string SectionName = "Scrape";

    [Required]
    [MinLength(1)]
    public List<ScrapeTargetOptions> Targets { get; init; } = [];

    public IReadOnlyList<CasinoTarget> ToTargets() =>
        Targets.Select(t => new CasinoTarget(t.Name, t.ReviewUrl!)).ToList();
}
