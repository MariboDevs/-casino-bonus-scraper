using System.Net;
using BonusScraper.Infrastructure.Crawling;
using BonusScraper.Infrastructure.UnitTests.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BonusScraper.Infrastructure.UnitTests.Crawling;

public sealed class RobotsPolicyTests
{
    private const string Robots = "https://x.example/robots.txt";
    private static readonly Uri Page = new("https://x.example/reviews/888");

    [Fact]
    public async Task Applies_the_published_rules()
    {
        var http = new StubHttp().Respond(Robots, HttpStatusCode.OK, "User-agent: *\nDisallow: /reviews/\nCrawl-delay: 3");

        var decision = await Policy(http).EvaluateAsync(Page, CancellationToken.None);

        decision.Allowed.ShouldBeFalse();
        decision.CrawlDelay.ShouldBe(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task Treats_a_missing_robots_txt_as_no_rules() =>
        (await Policy(new StubHttp().Respond(Robots, HttpStatusCode.NotFound)).EvaluateAsync(Page, CancellationToken.None))
            .Allowed.ShouldBeTrue();

    [Fact]
    public async Task Treats_a_server_error_as_disallowing_everything() =>
        (await Policy(new StubHttp().Respond(Robots, HttpStatusCode.ServiceUnavailable)).EvaluateAsync(Page, CancellationToken.None))
            .Allowed.ShouldBeFalse();

    [Fact]
    public async Task Treats_an_unreachable_site_as_disallowing_everything() =>
        (await Policy(new StubHttp().Fail(Robots)).EvaluateAsync(Page, CancellationToken.None))
            .Allowed.ShouldBeFalse();

    [Fact]
    public async Task Fetches_robots_txt_once_per_origin()
    {
        var http = new StubHttp().Respond(Robots, HttpStatusCode.OK, "User-agent: *\nAllow: /");
        var policy = Policy(http);

        await policy.EvaluateAsync(Page, CancellationToken.None);
        await policy.EvaluateAsync(new Uri("https://x.example/reviews/casumo"), CancellationToken.None);

        http.Requests.Count.ShouldBe(1);
    }

    private static RobotsPolicy Policy(StubHttp http) =>
        new(http, Options.Create(new CrawlerOptions()), NullLogger<RobotsPolicy>.Instance);
}
