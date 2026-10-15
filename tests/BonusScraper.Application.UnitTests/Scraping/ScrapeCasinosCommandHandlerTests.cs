using BonusScraper.Application.Abstractions;
using BonusScraper.Application.Scraping;
using BonusScraper.Domain.Runs;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace BonusScraper.Application.UnitTests.Scraping;

public sealed class ScrapeCasinosCommandHandlerTests
{
    private static readonly CasinoTarget EightEightEight = new("888 Casino", new Uri("https://reviews.example/888"));
    private static readonly CasinoTarget Grosvenor = new("Grosvenor Casino", new Uri("https://reviews.example/grosvenor"));

    private readonly IReviewPageSource _pages = Substitute.For<IReviewPageSource>();
    private readonly IBonusTermsExtractor _extractor = Substitute.For<IBonusTermsExtractor>();
    private readonly IScrapeRunRepository _runs = Substitute.For<IScrapeRunRepository>();
    private readonly IOfferPublisher _publisher = Substitute.For<IOfferPublisher>();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 14, 6, 0, 0, TimeSpan.Zero));

    public ScrapeCasinosCommandHandlerTests()
    {
        _runs.SaveAsync(Arg.Any<ScrapeRun>(), Arg.Any<CancellationToken>()).Returns(Result.Success);
        _publisher.PublishAsync(Arg.Any<ScrapeRun>(), Arg.Any<CancellationToken>()).Returns("data/casinos.json");
        GivenPage(EightEightEight, Page(EightEightEight, "Min £10 deposit. 100% bonus up to £200. 40x wagering."));
        GivenPage(Grosvenor, Page(Grosvenor, "Deposit £20 Get £30. Min deposit £20. £30 bonus (x30 wagering)."));
        GivenExtraction(EightEightEight, Terms("100% up to 200 GBP", 100, 200, 40, 10));
        GivenExtraction(Grosvenor, Terms("30 GBP bonus", null, 30, 30, 20));
    }

    [Fact]
    public async Task Records_saves_and_publishes_every_casino()
    {
        var result = await Handle(EightEightEight, Grosvenor);

        result.IsError.ShouldBeFalse();
        result.Value.Outcome.ShouldBe(ScrapeRunOutcome.Succeeded);
        result.Value.Offers.ShouldBe(2);
        result.Value.Provisional.ShouldBe(0);
        result.Value.PublishedTo.ShouldBe("data/casinos.json");
        await _runs.Received(1).SaveAsync(Arg.Is<ScrapeRun>(r => r.Offers.Count == 2 && r.CompletedAt != null), Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(Arg.Any<ScrapeRun>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Marks_an_offer_whose_model_answer_was_reused()
    {
        GivenExtraction(EightEightEight, Terms("100% up to 200 GBP", 100, 200, 40, 10) with { Reused = true });

        await Handle(EightEightEight, Grosvenor);

        await _runs.Received(1).SaveAsync(
            Arg.Is<ScrapeRun>(r =>
                r.Offers.Single(o => o.Casino.Value == EightEightEight.Name).AnswerReused
                && !r.Offers.Single(o => o.Casino.Value == Grosvenor.Name).AnswerReused),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Gives_the_extractor_the_same_text_the_offer_is_verified_against()
    {
        ScrapeRun? saved = null;
        await _runs.SaveAsync(Arg.Do<ScrapeRun>(r => saved = r), Arg.Any<CancellationToken>());

        await Handle(EightEightEight);

        await _extractor.Received(1).ExtractAsync(
            Arg.Is<BonusTermsDocument>(d => d.Casino == "888 Casino" && d.Text == saved!.Offers[0].Text.Full),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Counts_offers_the_terms_contradict_as_provisional()
    {
        GivenExtraction(EightEightEight, Terms("100% up to 200 GBP", 100, 200, 35, 10));

        var result = await Handle(EightEightEight);

        result.Value.Provisional.ShouldBe(1);
    }

    [Fact]
    public async Task Records_a_page_failure_and_carries_on()
    {
        GivenPage(EightEightEight, Error.Forbidden("Robots.Disallowed", "Blocked by robots.txt."));

        var result = await Handle(EightEightEight, Grosvenor);

        result.Value.Outcome.ShouldBe(ScrapeRunOutcome.PartiallySucceeded);
        result.Value.Offers.ShouldBe(1);
        result.Value.Failures.Single().ShouldBe(new ScrapeFailure("888 Casino", EightEightEight.ReviewUrl, "Robots.Disallowed", "Blocked by robots.txt."));
    }

    [Fact]
    public async Task Does_not_call_the_model_for_a_page_without_bonus_text()
    {
        GivenPage(EightEightEight, new ReviewPage(EightEightEight.ReviewUrl, null, " ", null, _clock.GetUtcNow()));

        var result = await Handle(EightEightEight, Grosvenor);

        result.Value.Failures.Single().Code.ShouldBe(ScrapeErrors.NoBonusText.Code);
        await _extractor.DidNotReceive().ExtractAsync(Arg.Is<BonusTermsDocument>(d => d.Casino == "888 Casino"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Records_an_extraction_failure()
    {
        GivenExtraction(EightEightEight, Error.Unexpected("Extractor.ModelUnavailable", "The model did not respond."));

        var result = await Handle(EightEightEight, Grosvenor);

        result.Value.Failures.Single().Code.ShouldBe("Extractor.ModelUnavailable");
    }

    [Fact]
    public async Task Records_a_model_answer_the_domain_rejects()
    {
        GivenExtraction(EightEightEight, Terms("100% up to 200 XYZ", 100, 200, 40, 10, currency: "XYZ"));

        var result = await Handle(EightEightEight, Grosvenor);

        result.Value.Failures.Single().Code.ShouldBe("Money.CurrencyUnknown");
    }

    [Fact]
    public async Task Leaves_published_data_untouched_when_nothing_was_scraped()
    {
        GivenPage(EightEightEight, Error.Unexpected("Http.Unavailable", "Down."));

        var result = await Handle(EightEightEight);

        result.FirstError.ShouldBe(ScrapeErrors.NothingScraped);
        await _runs.Received(1).SaveAsync(Arg.Is<ScrapeRun>(r => r.Outcome == ScrapeRunOutcome.Failed), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().PublishAsync(Arg.Any<ScrapeRun>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Does_not_publish_a_run_it_could_not_save()
    {
        var failure = Error.Unexpected("Database.Unavailable", "No connection.");
        _runs.SaveAsync(Arg.Any<ScrapeRun>(), Arg.Any<CancellationToken>()).Returns(failure);

        var result = await Handle(EightEightEight);

        result.FirstError.ShouldBe(failure);
        await _publisher.DidNotReceive().PublishAsync(Arg.Any<ScrapeRun>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_a_publishing_failure()
    {
        var failure = Error.Unexpected("Publish.WriteFailed", "Disk full.");
        _publisher.PublishAsync(Arg.Any<ScrapeRun>(), Arg.Any<CancellationToken>()).Returns(failure);

        var result = await Handle(EightEightEight);

        result.FirstError.ShouldBe(failure);
    }

    private Task<ErrorOr<ScrapeRunSummary>> Handle(params CasinoTarget[] targets) =>
        new ScrapeCasinosCommandHandler(_pages, _extractor, _runs, _publisher, _clock, NullLogger<ScrapeCasinosCommandHandler>.Instance)
            .Handle(new ScrapeCasinosCommand(targets), CancellationToken.None).AsTask();

    private void GivenPage(CasinoTarget target, ErrorOr<ReviewPage> page) =>
        _pages.ReadAsync(target.ReviewUrl, Arg.Any<CancellationToken>()).Returns(page);

    private void GivenExtraction(CasinoTarget target, ErrorOr<ExtractedBonusTerms> terms) =>
        _extractor.ExtractAsync(Arg.Is<BonusTermsDocument>(d => d.Casino == target.Name), Arg.Any<CancellationToken>()).Returns(terms);

    private ReviewPage Page(CasinoTarget target, string terms) =>
        new(target.ReviewUrl, null, terms, null, _clock.GetUtcNow());

    private static ExtractedBonusTerms Terms(
        string summary, int? match, decimal? max, decimal? wagering, decimal? minDeposit, string currency = "GBP") =>
        new(summary, match, max, null, wagering, minDeposit, currency, "test-model");
}
