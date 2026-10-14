namespace BonusScraper.Infrastructure.Extraction;

public sealed class AzureOpenAiOptions
{
    public Uri? Endpoint { get; init; }

    public string? Deployment { get; init; }

    /// <summary>Never committed: supplied through user secrets or the environment.</summary>
    public string? ApiKey { get; init; }

    public bool IsConfigured => Endpoint is not null && !string.IsNullOrWhiteSpace(Deployment) && !string.IsNullOrWhiteSpace(ApiKey);
}
