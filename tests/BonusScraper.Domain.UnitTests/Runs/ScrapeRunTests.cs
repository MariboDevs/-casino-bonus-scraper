using BonusScraper.Domain.Runs;

namespace BonusScraper.Domain.UnitTests.Runs;

public sealed class ScrapeRunTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 14, 6, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Fails_when_nothing_was_scraped()
    {
        var run = new ScrapeRun(Guid.NewGuid(), Start);
        run.Record(new ScrapeFailure("888", new Uri("https://example.com"), "Robots.Disallowed", "Blocked."));

        run.Outcome.ShouldBe(ScrapeRunOutcome.Failed);
    }

    [Fact]
    public void Cannot_complete_before_it_started() =>
        Should.Throw<ArgumentOutOfRangeException>(() => new ScrapeRun(Guid.NewGuid(), Start).Complete(Start.AddSeconds(-1)));

    [Fact]
    public void Is_immutable_once_completed()
    {
        var run = new ScrapeRun(Guid.NewGuid(), Start);
        run.Complete(Start.AddMinutes(1));

        Should.Throw<InvalidOperationException>(() =>
            run.Record(new ScrapeFailure("888", new Uri("https://example.com"), "X", "Y")));
        run.CompletedAt.ShouldBe(Start.AddMinutes(1));
    }
}
