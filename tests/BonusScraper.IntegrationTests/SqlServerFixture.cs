using BonusScraper.Migrator;
using Testcontainers.MsSql;

namespace BonusScraper.IntegrationTests;

/// <summary>One SQL Server 2022 container per test run, migrated with the real scripts. Needs Docker.</summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString().Replace("Database=master", "Database=bonusdb", StringComparison.Ordinal);

        var result = DatabaseMigrator.Migrate(ConnectionString, ensureDatabase: true);
        if (!result.Successful)
        {
            throw new InvalidOperationException($"Migration failed on {result.ErrorScript?.Name}.", result.Error);
        }
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
