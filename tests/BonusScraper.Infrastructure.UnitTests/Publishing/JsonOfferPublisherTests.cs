using System.Text.Json;
using BonusScraper.Domain.Monetary;
using BonusScraper.Domain.Offers;
using BonusScraper.Domain.Runs;
using BonusScraper.Infrastructure.Publishing;
using Microsoft.Extensions.Options;

namespace BonusScraper.Infrastructure.UnitTests.Publishing;

public sealed class JsonOfferPublisherTests : IDisposable
{
    private static readonly DateTimeOffset Start = new(2026, 10, 14, 6, 0, 0, TimeSpan.Zero);

    private readonly string _path = Path.Combine(Path.GetTempPath(), "bonus-publish-" + Guid.NewGuid().ToString("N"), "casinos.json");

    [Fact]
    public async Task Publishes_the_four_brief_fields_flat_with_details_alongside()
    {
        var result = await Publisher().PublishAsync(Run(), CancellationToken.None);

        result.Value.ShouldBe(_path);
        using var json = JsonDocument.Parse(await File.ReadAllTextAsync(_path));
        var root = json.RootElement;
        root.GetProperty("schemaVersion").GetInt32().ShouldBe(PublishedOffersSerializer.SchemaVersion);
        root.GetProperty("source").GetString().ShouldBe("casinoguide.co.uk");
        root.GetProperty("outcome").GetString().ShouldBe("partiallySucceeded");

        var casino = root.GetProperty("casinos")[0];
        casino.GetProperty("casino").GetString().ShouldBe("888 Casino");
        casino.GetProperty("welcomeBonus").GetString().ShouldBe("100% up to 200 GBP");
        casino.GetProperty("wageringRequirement").GetDecimal().ShouldBe(40m);
        casino.GetProperty("minimumDeposit").GetString().ShouldBe("10 GBP");
        casino.GetProperty("sourceUrl").GetString().ShouldBe("https://www.casinoguide.co.uk/888-casino-review");
        casino.GetProperty("details").GetProperty("maxBonus").GetProperty("currency").GetString().ShouldBe("GBP");
        casino.GetProperty("verification").GetProperty("status").GetString().ShouldBe("confirmed");

        var failure = root.GetProperty("failures")[0];
        failure.GetProperty("casino").GetString().ShouldBe("Casumo");
        failure.GetProperty("code").GetString().ShouldBe("Source.Blocked");
    }

    [Fact]
    public async Task Leaves_no_temporary_file_behind()
    {
        await Publisher().PublishAsync(Run(), CancellationToken.None);

        File.Exists(_path + ".tmp").ShouldBeFalse();
    }

    public void Dispose()
    {
        var directory = Path.GetDirectoryName(_path)!;
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private JsonOfferPublisher Publisher() =>
        new(Options.Create(new PublishingOptions { OutputPath = _path }));

    private static ScrapeRun Run()
    {
        var run = new ScrapeRun(Guid.NewGuid(), Start);
        var gbp200 = Money.Create(200, "GBP").Value;
        run.Record(BonusOffer.Create(
            CasinoName.Create("888 Casino").Value,
            WelcomeBonus.Create("100% up to 200 GBP", 100, gbp200, null).Value,
            WageringRequirement.Create(40).Value,
            Money.Create(10, "GBP").Value,
            SourceUrl.Create(new Uri("https://www.casinoguide.co.uk/888-casino-review")).Value,
            "Min £10 deposit. 100% bonus up to £200. 40x wagering.",
            "test-model",
            Start).Value);
        run.Record(new ScrapeFailure("Casumo", new Uri("https://www.casinoguide.co.uk/casumo-casino-review"), "Source.Blocked", "Refused."));
        run.Complete(Start.AddMinutes(1));
        return run;
    }
}
