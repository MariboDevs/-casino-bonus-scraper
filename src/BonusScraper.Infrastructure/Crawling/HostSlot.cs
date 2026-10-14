namespace BonusScraper.Infrastructure.Crawling;

/// <summary>Per-host politeness state for <see cref="PoliteCrawlHandler"/>.</summary>
internal sealed class HostSlot
{
    public SemaphoreSlim Gate { get; } = new(1, 1);

    public DateTimeOffset? LastRequestAt { get; set; }
}
