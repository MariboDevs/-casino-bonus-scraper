using System.CommandLine;
using BonusScraper.Application;
using BonusScraper.Application.Behaviours;
using BonusScraper.Application.Scraping;
using BonusScraper.Cli;
using BonusScraper.Infrastructure;
using BonusScraper.Infrastructure.Extraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

var provider = new Option<string?>("--provider") { Description = "Extraction provider: AzureOpenAI, Anthropic or Replay." };
var output = new Option<string?>("--output") { Description = "Where to write the offers JSON (default data/casinos.json)." };
var recordTo = new Option<string?>("--record-to") { Description = "Record live model answers to this directory for later replay." };

var scrape = new Command("scrape", "Scrape every configured casino, extract its welcome offer and publish the JSON.")
{
    provider,
    output,
    recordTo,
};

scrape.SetAction(async (parse, cancellationToken) =>
{
    var providerName = parse.GetValue(provider);
    if (providerName is not null && !Enum.TryParse<ExtractionProvider>(providerName, ignoreCase: true, out _))
    {
        Log.Fatal("Unknown --provider {Provider}. Use one of: {Providers}", providerName, string.Join(", ", Enum.GetNames<ExtractionProvider>()));
        await Log.CloseAndFlushAsync();
        return ExitCodes.InvalidConfiguration;
    }

    var overrides = new Dictionary<string, string?>
    {
        ["Extraction:Provider"] = parse.GetValue(provider),
        ["Extraction:RecordTo"] = parse.GetValue(recordTo),
        ["Publishing:OutputPath"] = parse.GetValue(output),
    }.Where(kv => kv.Value is not null);

    try
    {
        using var host = BuildHost(overrides);
        await host.StartAsync(cancellationToken);
        await using var scope = host.Services.CreateAsyncScope();
        var exitCode = await scope.ServiceProvider.GetRequiredService<ScrapeCommandRunner>().RunAsync(cancellationToken);
        await host.StopAsync(cancellationToken);
        return exitCode;
    }
    catch (OptionsValidationException ex)
    {
        Log.Fatal("Invalid configuration: {Failures}", string.Join("; ", ex.Failures));
        return ExitCodes.InvalidConfiguration;
    }
    catch (InvalidOperationException ex)
    {
        Log.Fatal("Invalid configuration: {Message}", ex.Message);
        return ExitCodes.InvalidConfiguration;
    }
    finally
    {
        await Log.CloseAndFlushAsync();
    }
});

var root = new RootCommand("Scrapes casino welcome bonuses, extracts them with a language model and publishes them as JSON.")
{
    scrape,
};

return await root.Parse(args).InvokeAsync();

static IHost BuildHost(IEnumerable<KeyValuePair<string, string?>> overrides)
{
    // Settings ship next to the binary; relative output paths still resolve from the
    // working directory, so "data/casinos.json" lands in the repository when run from its root.
    var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { ContentRootPath = AppContext.BaseDirectory });
    builder.Configuration.AddUserSecrets(typeof(ScrapeOptions).Assembly, optional: true);
    builder.Configuration.AddInMemoryCollection(overrides);

    builder.Services.AddSerilog((services, logger) => logger
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddOptions<ScrapeOptions>()
        .Bind(builder.Configuration.GetSection(ScrapeOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddMediator((Mediator.MediatorOptions options) =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies = [typeof(ScrapeCasinosCommand)];
            options.PipelineBehaviors = [typeof(LoggingBehaviour<,>), typeof(ValidationBehaviour<,>)];
        });

    builder.Services.AddScoped<ScrapeCommandRunner>();

    return builder.Build();
}
