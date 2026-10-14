using System.ComponentModel.DataAnnotations;

namespace BonusScraper.Cli;

public sealed class ScrapeTargetOptions
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public Uri? ReviewUrl { get; init; }
}
