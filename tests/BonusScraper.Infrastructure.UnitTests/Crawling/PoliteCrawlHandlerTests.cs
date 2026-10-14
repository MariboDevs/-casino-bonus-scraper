using System.Net;
using BonusScraper.Infrastructure.Crawling;
using BonusScraper.Infrastructure.UnitTests.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace BonusScraper.Infrastructure.UnitTests.Crawling;

public sealed class PoliteCrawlHandlerTests
{
    private readonly FakeTimeProvider _clock = new();
    private readonly IRobotsPolicy _robots = Substitute.For<IRobotsPolicy>();

    [Fact]
    public async Task Waits_the_crawl_delay_between_requests_to_one_host()
    {
        _robots.EvaluateAsync(Arg.Any<Uri>(), Arg.Any<CancellationToken>()).Returns(new RobotsDecision(true, TimeSpan.FromSeconds(5)));
        using var client = Client(minimumDelaySeconds: 1);

        await client.GetAsync(new Uri("https://x.example/a"));
        var second = client.GetAsync(new Uri("https://x.example/b"));

        _clock.Advance(TimeSpan.FromSeconds(4));
        second.IsCompleted.ShouldBeFalse();
        _clock.Advance(TimeSpan.FromSeconds(1));
        (await second).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Never_waits_less_than_the_configured_minimum()
    {
        _robots.EvaluateAsync(Arg.Any<Uri>(), Arg.Any<CancellationToken>()).Returns(new RobotsDecision(true, null));
        using var client = Client(minimumDelaySeconds: 2);

        await client.GetAsync(new Uri("https://x.example/a"));
        var second = client.GetAsync(new Uri("https://x.example/b"));

        _clock.Advance(TimeSpan.FromSeconds(1));
        second.IsCompleted.ShouldBeFalse();
        _clock.Advance(TimeSpan.FromSeconds(1));
        await second;
    }

    [Fact]
    public async Task Does_not_delay_requests_to_different_hosts()
    {
        _robots.EvaluateAsync(Arg.Any<Uri>(), Arg.Any<CancellationToken>()).Returns(new RobotsDecision(true, TimeSpan.FromSeconds(30)));
        using var client = Client(minimumDelaySeconds: 2);

        await client.GetAsync(new Uri("https://x.example/a"));
        var other = client.GetAsync(new Uri("https://y.example/a"));

        other.IsCompleted.ShouldBeTrue();
        await other;
    }

    private HttpClient Client(double minimumDelaySeconds)
    {
        var stub = new StubHttp()
            .Respond("https://x.example/a", HttpStatusCode.OK)
            .Respond("https://x.example/b", HttpStatusCode.OK)
            .Respond("https://y.example/a", HttpStatusCode.OK);
        var handler = new PoliteCrawlHandler(_robots, Options.Create(new CrawlerOptions { MinimumDelaySeconds = minimumDelaySeconds }), _clock)
        {
            InnerHandler = stub,
        };
        return new HttpClient(handler);
    }
}
