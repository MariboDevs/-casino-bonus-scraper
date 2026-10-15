using BonusScraper.Domain.Monetary;
using BonusScraper.Domain.Offers;
using BonusScraper.Domain.Runs;
using BonusScraper.Infrastructure.Persistence;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BonusScraper.IntegrationTests;

[Collection(SqlServerCollection.Name)]
public sealed class SqlScrapeRunRepositoryTests(SqlServerFixture database)
{
    private static readonly DateTimeOffset Start = new(2026, 10, 14, 6, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Saves_a_run_with_its_offers_and_failures()
    {
        var run = Run(Start, wagering: 40);

        var result = await Repository().SaveAsync(run, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        await using var connection = new SqlConnection(database.ConnectionString);
        (await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM scrape.BonusOffer WHERE ScrapeRunId = @Id", new { run.Id })).ShouldBe(1);
        (await connection.ExecuteScalarAsync<string>("SELECT ErrorCode FROM scrape.ScrapeFailure WHERE ScrapeRunId = @Id", new { run.Id })).ShouldBe("Source.Blocked");
        (await connection.ExecuteScalarAsync<string>("SELECT Outcome FROM scrape.ScrapeRun WHERE ScrapeRunId = @Id", new { run.Id })).ShouldBe("PartiallySucceeded");
    }

    [Fact]
    public async Task Keeps_one_casino_row_and_exposes_its_latest_offer()
    {
        var casino = "History Casino " + Guid.NewGuid().ToString("N")[..8];
        await Repository().SaveAsync(Run(Start, wagering: 35, casino), CancellationToken.None);
        await Repository().SaveAsync(Run(Start.AddDays(1), wagering: 40, casino), CancellationToken.None);

        await using var connection = new SqlConnection(database.ConnectionString);
        (await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM scrape.Casino WHERE Name = @casino", new { casino })).ShouldBe(1);
        (await connection.ExecuteScalarAsync<decimal>("SELECT WageringMultiplier FROM scrape.LatestBonusOffer WHERE Casino = @casino", new { casino })).ShouldBe(40m);
    }

    private SqlScrapeRunRepository Repository() => new(new SqlConnectionFactory(database.ConnectionString));

    private static ScrapeRun Run(DateTimeOffset at, decimal wagering, string casino = "888 Casino")
    {
        var run = new ScrapeRun(Guid.NewGuid(), at);
        run.Record(BonusOffer.Create(
            CasinoName.Create(casino).Value,
            WelcomeBonus.Create("100% up to 200 GBP", 100, Money.Create(200, "GBP").Value, null).Value,
            WageringRequirement.Create(wagering).Value,
            Money.Create(10, "GBP").Value,
            SourceUrl.Create(new Uri("https://www.casinoguide.co.uk/888-casino-review")).Value,
            OfferText.Create("Min £10 deposit. 100% bonus up to £200. 40x wagering.", null).Value,
            "test-model",
            at).Value);
        run.Record(new ScrapeFailure("Casumo", new Uri("https://www.casinoguide.co.uk/casumo-casino-review"), "Source.Blocked", "Refused."));
        run.Complete(at.AddMinutes(1));
        return run;
    }
}
