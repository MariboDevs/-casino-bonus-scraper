using BonusScraper.Migrator;
using Microsoft.Extensions.Configuration;

// A one-shot console: Aspire runs it before the scraper, and the pipeline runs it before a deploy.
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

var connectionString = configuration.GetConnectionString("bonusdb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    await Console.Error.WriteLineAsync("ConnectionStrings:bonusdb is required.");
    return 2;
}

var result = DatabaseMigrator.Migrate(connectionString, ensureDatabase: configuration.GetValue("EnsureDatabase", true));
if (!result.Successful)
{
    await Console.Error.WriteLineAsync($"Migration failed on {result.ErrorScript?.Name}: {result.Error?.Message}");
    return 1;
}

Console.WriteLine("Database is up to date.");
return 0;
