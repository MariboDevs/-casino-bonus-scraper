using DbUp;
using DbUp.Engine;

namespace BonusScraper.Migrator;

/// <summary>Applies the embedded, numbered scripts in order. Each script runs once, in its own transaction.</summary>
public static class DatabaseMigrator
{
    public static DatabaseUpgradeResult Migrate(string connectionString, bool ensureDatabase)
    {
        if (ensureDatabase)
        {
            EnsureDatabase.For.SqlDatabase(connectionString);
        }

        return DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(DatabaseMigrator).Assembly)
            .JournalToSqlTable("dbo", "SchemaVersions")
            .WithTransactionPerScript()
            .LogToConsole()
            .Build()
            .PerformUpgrade();
    }
}
