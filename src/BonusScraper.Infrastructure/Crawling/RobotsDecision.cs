namespace BonusScraper.Infrastructure.Crawling;

internal sealed record RobotsDecision(bool Allowed, TimeSpan? CrawlDelay);
