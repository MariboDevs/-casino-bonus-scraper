using BonusScraper.Application.Abstractions;
using BonusScraper.Infrastructure.Crawling;
using BonusScraper.Infrastructure.Extraction;
using BonusScraper.Infrastructure.Persistence;
using BonusScraper.Infrastructure.Publishing;
using BonusScraper.Infrastructure.Sources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

namespace BonusScraper.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CrawlerOptions>().Bind(configuration.GetSection(CrawlerOptions.SectionName)).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<ExtractionOptions>().Bind(configuration.GetSection(ExtractionOptions.SectionName)).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<PublishingOptions>().Bind(configuration.GetSection(PublishingOptions.SectionName)).ValidateDataAnnotations().ValidateOnStart();

        AddCrawling(services);
        AddExtraction(services);
        AddPersistence(services, configuration);
        services.AddSingleton<IOfferPublisher>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<PublishingOptions>>();
            var file = new JsonOfferPublisher(options);
            return options.Value.BlobContainerUrl is null
                ? file
                : new CompositeOfferPublisher([file, new BlobOfferPublisher(options)]);
        });

        return services;
    }

    private static void AddCrawling(IServiceCollection services)
    {
        services.AddSingleton<IRobotsPolicy, RobotsPolicy>();
        services.AddTransient<PoliteCrawlHandler>();
        services.AddSingleton<IReviewPageSource, CasinoGuideReviewPageSource>();

        services.AddHttpClient(RobotsPolicy.ClientName, Configure).AddStandardResilienceHandler(Resilience);
        // Resilience is registered first so it is the outer handler: every retry passes
        // back through the politeness gate instead of skipping the per-host delay.
        services.AddHttpClient(CasinoGuideReviewPageSource.ClientName, Configure)
            .AddStandardResilienceHandler(Resilience)
            .Services.AddHttpClient(CasinoGuideReviewPageSource.ClientName)
            .AddHttpMessageHandler<PoliteCrawlHandler>();

        static void Configure(IServiceProvider provider, HttpClient client)
        {
            var options = provider.GetRequiredService<IOptions<CrawlerOptions>>().Value;
            client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,text/plain;q=0.9");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds * 3);
        }

        // Two retries with backoff for transient faults only. A 403 is never retried: being
        // refused is an answer, and the brief is to switch source rather than push past it.
        static void Resilience(HttpStandardResilienceOptions options)
        {
            options.Retry.MaxRetryAttempts = 2;
            options.Retry.UseJitter = true;
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(20);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(90);
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
        }
    }

    private static void AddExtraction(IServiceCollection services) =>
        services.AddSingleton<IBonusTermsExtractor>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<ExtractionOptions>>();
            IBonusTermsExtractor extractor = options.Value.Provider switch
            {
                ExtractionProvider.AzureOpenAI => new AzureOpenAiBonusTermsExtractor(options),
                ExtractionProvider.Anthropic => new ClaudeBonusTermsExtractor(options),
                ExtractionProvider.Replay => new ReplayBonusTermsExtractor(options),
                _ => throw new InvalidOperationException($"Unknown extraction provider {options.Value.Provider}."),
            };

            return options.Value is { RecordTo: { Length: > 0 } directory, Provider: not ExtractionProvider.Replay }
                ? new RecordingBonusTermsExtractor(extractor, directory)
                : extractor;
        });

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(PersistenceOptions.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddSingleton<IScrapeRunRepository, NullScrapeRunRepository>();
            return;
        }

        services.AddSingleton<ISqlConnectionFactory>(new SqlConnectionFactory(connectionString));
        services.AddSingleton<IScrapeRunRepository, SqlScrapeRunRepository>();
    }
}
