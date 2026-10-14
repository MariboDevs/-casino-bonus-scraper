using BonusScraper.Application.Abstractions;
using BonusScraper.Domain.Runs;
using BonusScraper.Infrastructure.Publishing;
using ErrorOr;

namespace BonusScraper.Infrastructure.UnitTests.Publishing;

public sealed class CompositeOfferPublisherTests
{
    private static readonly ScrapeRun Run = new(Guid.NewGuid(), DateTimeOffset.UnixEpoch);

    [Fact]
    public async Task Publishes_to_every_target_and_reports_each_location()
    {
        var result = await new CompositeOfferPublisher([Publisher("data/casinos.json"), Publisher("https://blob/offers/casinos.json")])
            .PublishAsync(Run, CancellationToken.None);

        result.Value.ShouldBe("data/casinos.json, https://blob/offers/casinos.json");
    }

    [Fact]
    public async Task Fails_when_any_target_fails_and_stops_there()
    {
        var failing = Substitute.For<IOfferPublisher>();
        failing.PublishAsync(Run, Arg.Any<CancellationToken>()).Returns(Error.Unexpected("Publish.WriteFailed", "Disk full."));
        var later = Publisher("never");

        var result = await new CompositeOfferPublisher([failing, later]).PublishAsync(Run, CancellationToken.None);

        result.FirstError.Code.ShouldBe("Publish.WriteFailed");
        await later.DidNotReceive().PublishAsync(Arg.Any<ScrapeRun>(), Arg.Any<CancellationToken>());
    }

    private static IOfferPublisher Publisher(string location)
    {
        var publisher = Substitute.For<IOfferPublisher>();
        publisher.PublishAsync(Run, Arg.Any<CancellationToken>()).Returns(location);
        return publisher;
    }
}
