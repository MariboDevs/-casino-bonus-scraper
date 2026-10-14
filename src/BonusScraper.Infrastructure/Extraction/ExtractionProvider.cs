namespace BonusScraper.Infrastructure.Extraction;

public enum ExtractionProvider
{
    /// <summary>Azure OpenAI chat completions with a strict JSON schema response format.</summary>
    AzureOpenAI,

    /// <summary>The Claude API with structured outputs.</summary>
    Anthropic,

    /// <summary>Answers previously recorded from a live provider. Offline, free and deterministic.</summary>
    Replay,
}
