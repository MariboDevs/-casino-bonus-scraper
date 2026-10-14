using BonusScraper.Domain.Offers;

namespace BonusScraper.Domain.Runs;

/// <summary>One execution of the scraper across every configured casino.</summary>
public sealed class ScrapeRun
{
    private readonly List<BonusOffer> _offers = [];
    private readonly List<ScrapeFailure> _failures = [];

    public ScrapeRun(Guid id, DateTimeOffset startedAt)
    {
        Id = id;
        StartedAt = startedAt;
    }

    public Guid Id { get; }

    public DateTimeOffset StartedAt { get; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public IReadOnlyList<BonusOffer> Offers => _offers;

    public IReadOnlyList<ScrapeFailure> Failures => _failures;

    public ScrapeRunOutcome Outcome => (_offers.Count, _failures.Count) switch
    {
        (0, _) => ScrapeRunOutcome.Failed,
        (_, 0) => ScrapeRunOutcome.Succeeded,
        _ => ScrapeRunOutcome.PartiallySucceeded,
    };

    public void Record(BonusOffer offer)
    {
        EnsureOpen();
        _offers.Add(offer);
    }

    public void Record(ScrapeFailure failure)
    {
        EnsureOpen();
        _failures.Add(failure);
    }

    public void Complete(DateTimeOffset completedAt)
    {
        EnsureOpen();
        if (completedAt < StartedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(completedAt), "A run cannot complete before it started.");
        }

        CompletedAt = completedAt;
    }

    private void EnsureOpen()
    {
        if (CompletedAt is not null)
        {
            throw new InvalidOperationException("A completed run is immutable.");
        }
    }
}
