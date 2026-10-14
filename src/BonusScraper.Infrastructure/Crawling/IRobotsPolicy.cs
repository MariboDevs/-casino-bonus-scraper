namespace BonusScraper.Infrastructure.Crawling;

internal interface IRobotsPolicy
{
    Task<RobotsDecision> EvaluateAsync(Uri uri, CancellationToken cancellationToken);
}
