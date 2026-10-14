using System.ComponentModel.DataAnnotations;

namespace BonusScraper.Infrastructure.Crawling;

public sealed class CrawlerOptions
{
    public const string SectionName = "Crawler";

    /// <summary>The robots.txt product token this crawler identifies as.</summary>
    [Required]
    [RegularExpression("^[A-Za-z_-]+$")]
    public string ProductToken { get; init; } = "BonusScraperBot";

    /// <summary>Where a site owner can find out who is crawling them.</summary>
    [Required]
    [Url]
    public string ContactUrl { get; init; } = "https://github.com/MariboDevs/-casino-bonus-scraper";

    /// <summary>The minimum gap between two requests to one host, even if robots.txt asks for less.</summary>
    [Range(0, 60)]
    public double MinimumDelaySeconds { get; init; } = 2;

    /// <summary>A robots.txt Crawl-delay above this is treated as "do not crawl this host now".</summary>
    [Range(1, 600)]
    public double MaximumDelaySeconds { get; init; } = 30;

    [Range(1, 120)]
    public int TimeoutSeconds { get; init; } = 20;

    public string UserAgent => $"{ProductToken}/1.0 (+{ContactUrl})";
}
