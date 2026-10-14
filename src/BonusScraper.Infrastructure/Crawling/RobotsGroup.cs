namespace BonusScraper.Infrastructure.Crawling;

internal sealed record RobotsGroup(IReadOnlyList<string> UserAgents, IReadOnlyList<RobotsRule> Rules, TimeSpan? CrawlDelay);
