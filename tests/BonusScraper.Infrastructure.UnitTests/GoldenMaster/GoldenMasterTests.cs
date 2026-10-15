using System.Net;
using System.Text.Json;
using BonusScraper.Application.Scraping;
using BonusScraper.Infrastructure.Crawling;
using BonusScraper.Infrastructure.Extraction;
using BonusScraper.Infrastructure.Persistence;
using BonusScraper.Infrastructure.Publishing;
using BonusScraper.Infrastructure.Sources;
using BonusScraper.Infrastructure.UnitTests.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace BonusScraper.Infrastructure.UnitTests.GoldenMaster;

/// <summary>
/// The whole pipeline, offline: the review pages as captured, the answers the live model
/// (gpt-5.5 on Azure OpenAI) actually gave for them on 2026-10-15, and the real parser,
/// cross-check and publisher in between. If any stage changes what reaches the page, this
/// fails. A deliberate change updates expected-casinos.json in the same commit.
/// </summary>
public sealed class GoldenMasterTests : IDisposable
{
    private const string Origin = "https://www.casinoguide.co.uk";

    private static readonly CasinoTarget[] Targets =
    [
        new("888 Casino", new Uri($"{Origin}/888-casino-review")),
        new("Casumo", new Uri($"{Origin}/casumo-casino-review")),
        new("Grosvenor Casino", new Uri($"{Origin}/grosvenor-casino-review")),
    ];

    private readonly string _output = Path.Combine(Path.GetTempPath(), "golden-" + Guid.NewGuid().ToString("N"), "casinos.json");

    [Fact]
    public async Task Recorded_model_answers_still_produce_the_published_offers()
    {
        var result = await Handler().Handle(new ScrapeCasinosCommand(Targets), CancellationToken.None);

        result.IsError.ShouldBeFalse(string.Join(", ", result.ErrorsOrEmptyList.Select(e => e.Code)));
        Normalise(await File.ReadAllTextAsync(_output)).ShouldBe(Normalise(Fixture.Read("expected-casinos.json")));
    }

    public void Dispose()
    {
        var directory = Path.GetDirectoryName(_output)!;
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private ScrapeCasinosCommandHandler Handler()
    {
        var http = new StubHttp()
            .Respond($"{Origin}/robots.txt", HttpStatusCode.OK, Fixture.Read("casinoguide-robots.txt"))
            .Respond($"{Origin}/888-casino-review", HttpStatusCode.OK, Fixture.Page("888"))
            .Respond($"{Origin}/casumo-casino-review", HttpStatusCode.OK, Fixture.Page("casumo"))
            .Respond($"{Origin}/grosvenor-casino-review", HttpStatusCode.OK, Fixture.Page("grosvenor"));
        var crawler = Options.Create(new CrawlerOptions());
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 15, 5, 17, 0, TimeSpan.Zero));
        var robots = new RobotsPolicy(http, crawler, NullLogger<RobotsPolicy>.Instance);

        return new ScrapeCasinosCommandHandler(
            new CasinoGuideReviewPageSource(http, robots, crawler, clock),
            new ReplayBonusTermsExtractor(Options.Create(new ExtractionOptions
            {
                Provider = ExtractionProvider.Replay,
                RecordingsPath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "recordings"),
            })),
            new NullScrapeRunRepository(),
            new JsonOfferPublisher(Options.Create(new PublishingOptions { OutputPath = _output })),
            clock,
            NullLogger<ScrapeCasinosCommandHandler>.Instance);
    }

    /// <summary>Drops the fields that differ on every run (identifiers and timestamps) before comparing.</summary>
    private static string Normalise(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var stable = new
        {
            schemaVersion = root.GetProperty("schemaVersion").GetInt32(),
            source = root.GetProperty("source").GetString(),
            outcome = root.GetProperty("outcome").GetString(),
            casinos = root.GetProperty("casinos").EnumerateArray().Select(c => new
            {
                casino = c.GetProperty("casino").GetString(),
                welcomeBonus = c.GetProperty("welcomeBonus").GetString(),
                wageringRequirement = c.GetProperty("wageringRequirement").ToString(),
                minimumDeposit = c.GetProperty("minimumDeposit").ToString(),
                sourceUrl = c.GetProperty("sourceUrl").GetString(),
                details = c.GetProperty("details").GetRawText(),
                verification = c.GetProperty("verification").GetRawText(),
                extractedBy = c.GetProperty("extractedBy").GetString(),
            }).ToList(),
            failures = root.GetProperty("failures").GetRawText(),
        };

        return JsonSerializer.Serialize(stable, new JsonSerializerOptions { WriteIndented = true });
    }
}
