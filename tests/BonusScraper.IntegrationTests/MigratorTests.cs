using BonusScraper.Migrator;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BonusScraper.IntegrationTests;

[Collection(SqlServerCollection.Name)]
public sealed class MigratorTests(SqlServerFixture database)
{
    [Fact]
    public async Task Creates_the_scrape_schema()
    {
        await using var connection = new SqlConnection(database.ConnectionString);

        var tables = await connection.QueryAsync<string>(
            "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'scrape' ORDER BY TABLE_NAME");

        tables.ShouldBe(["BonusOffer", "Casino", "LatestBonusOffer", "ScrapeFailure", "ScrapeRun"]);
        (await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'scrape' AND TABLE_NAME = 'BonusOffer' AND COLUMN_NAME = 'VerificationBasis'"))
            .ShouldBe(1);
    }

    [Fact]
    public void Is_a_no_op_when_run_again()
    {
        var result = DatabaseMigrator.Migrate(database.ConnectionString, ensureDatabase: false);

        result.Successful.ShouldBeTrue();
        result.Scripts.ShouldBeEmpty();
    }
}
