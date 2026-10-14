namespace BonusScraper.Infrastructure.Extraction;

public sealed class AzureOpenAiOptions
{
    public Uri? Endpoint { get; init; }

    public string? Deployment { get; init; }

    /// <summary>
    /// Optional, and never committed. Without it the extractor authenticates with Entra ID
    /// (a managed identity in Azure, the developer's login locally), which is the
    /// production setting: no key exists to leak or rotate.
    /// </summary>
    public string? ApiKey { get; init; }

    public bool IsConfigured => Endpoint is not null && !string.IsNullOrWhiteSpace(Deployment);

    public bool UsesKey => !string.IsNullOrWhiteSpace(ApiKey);
}
