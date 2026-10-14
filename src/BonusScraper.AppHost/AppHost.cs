// The whole stack on one machine: SQL Server and Seq in containers, the migrator as a
// one-shot that must succeed first, then the scraper writing to both the database and
// the JSON file, and the Angular page served from that JSON.
var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql").WithDataVolume();
var database = sql.AddDatabase("bonusdb");
var seq = builder.AddSeq("seq");

var migrator = builder.AddProject<Projects.BonusScraper_Migrator>("migrator")
    .WithReference(database)
    .WaitFor(database);

var scraper = builder.AddProject<Projects.BonusScraper_Cli>("scraper")
    .WithArgs("scrape", "--output", Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "..", "data", "casinos.json")))
    .WithReference(database)
    .WithReference(seq)
    .WaitForCompletion(migrator)
    .WithEnvironment("Extraction__Provider", builder.Configuration["Extraction:Provider"] ?? "Replay")
    .WithEnvironment("Extraction__RecordingsPath", Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "..", "recordings")));

builder.AddJavaScriptApp("web", "../../frontend", "start")
    .WithHttpEndpoint(env: "PORT")
    .WaitForCompletion(scraper);

builder.Build().Run();
