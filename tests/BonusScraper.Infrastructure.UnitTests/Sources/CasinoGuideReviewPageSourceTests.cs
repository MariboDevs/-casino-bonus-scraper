using System.Net;
using BonusScraper.Infrastructure.Crawling;
using BonusScraper.Infrastructure.Sources;
using BonusScraper.Infrastructure.UnitTests.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace BonusScraper.Infrastructure.UnitTests.Sources;

public sealed class CasinoGuideReviewPageSourceTests
{
    private const string Url = "https://www.casinoguide.co.uk/888-casino-review";
    private static readonly Uri Page = new(Url);

    private readonly IRobotsPolicy _robots = Substitute.For<IRobotsPolicy>();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 14, 6, 0, 0, TimeSpan.Zero));

    public CasinoGuideReviewPageSourceTests() =>
        _robots.EvaluateAsync(Arg.Any<Uri>(), Arg.Any<CancellationToken>()).Returns(new RobotsDecision(true, null));

    [Fact]
    public async Task Reads_the_offer_from_a_review_page()
    {
        var page = (await Source(new StubHttp().Respond(Url, HttpStatusCode.OK, Fixture.Page("888"))).ReadAsync(Page, CancellationToken.None)).Value;

        page.Source.ShouldBe(Page);
        page.Headline.ShouldBe("100% up to £200");
        page.FetchedAt.ShouldBe(_clock.GetUtcNow());
        page.CombinedText.ShouldStartWith("100% up to £200\n\n18+ | For new 888casino");
    }

    [Fact]
    public async Task Does_not_request_a_page_robots_txt_disallows()
    {
        var http = new StubHttp();
        _robots.EvaluateAsync(Page, Arg.Any<CancellationToken>()).Returns(new RobotsDecision(false, null));

        var result = await Source(http).ReadAsync(Page, CancellationToken.None);

        result.FirstError.ShouldBe(SourceErrors.RobotsDisallowed);
        http.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task Does_not_crawl_a_site_asking_for_more_patience_than_configured()
    {
        _robots.EvaluateAsync(Page, Arg.Any<CancellationToken>()).Returns(new RobotsDecision(true, TimeSpan.FromMinutes(5)));

        var result = await Source(new StubHttp()).ReadAsync(Page, CancellationToken.None);

        result.FirstError.Code.ShouldBe("Source.CrawlDelayTooLong");
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "Source.Blocked")]
    [InlineData(HttpStatusCode.TooManyRequests, "Source.Blocked")]
    [InlineData(HttpStatusCode.NotFound, "Source.PageNotFound")]
    [InlineData(HttpStatusCode.BadGateway, "Source.Failed")]
    public async Task Maps_refusals_and_failures_to_errors(HttpStatusCode status, string code) =>
        (await Source(new StubHttp().Respond(Url, status)).ReadAsync(Page, CancellationToken.None))
            .FirstError.Code.ShouldBe(code);

    [Fact]
    public async Task Reports_an_unreachable_site() =>
        (await Source(new StubHttp().Fail(Url)).ReadAsync(Page, CancellationToken.None))
            .FirstError.ShouldBe(SourceErrors.Unreachable);

    [Fact]
    public async Task Recognises_a_bot_challenge_instead_of_bypassing_it()
    {
        const string challenge = "<html><head><title>Just a moment...</title></head><body><div id=\"challenge-form\"></div></body></html>";

        var result = await Source(new StubHttp().Respond(Url, HttpStatusCode.OK, challenge)).ReadAsync(Page, CancellationToken.None);

        result.FirstError.ShouldBe(SourceErrors.ChallengePage);
    }

    [Fact]
    public async Task Does_not_mistake_a_review_with_a_captcha_comment_form_for_a_challenge()
    {
        var html = Fixture.Page("888").Replace("</body>", "<div class=\"g-recaptcha\"></div></body>", StringComparison.Ordinal);

        var result = await Source(new StubHttp().Respond(Url, HttpStatusCode.OK, html)).ReadAsync(Page, CancellationToken.None);

        result.IsError.ShouldBeFalse();
    }

    private CasinoGuideReviewPageSource Source(StubHttp http) =>
        new(http, _robots, Options.Create(new CrawlerOptions()), _clock);
}
