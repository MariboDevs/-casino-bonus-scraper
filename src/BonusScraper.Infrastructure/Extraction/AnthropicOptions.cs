namespace BonusScraper.Infrastructure.Extraction;

public sealed class AnthropicOptions
{
    public string Model { get; init; } = "claude-opus-5-5";

    /// <summary>Never committed: supplied through user secrets or the environment.</summary>
    public string? ApiKey { get; init; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
