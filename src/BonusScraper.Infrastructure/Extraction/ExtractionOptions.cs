using System.ComponentModel.DataAnnotations;

namespace BonusScraper.Infrastructure.Extraction;

public sealed class ExtractionOptions
{
    public const string SectionName = "Extraction";

    public ExtractionProvider Provider { get; init; } = ExtractionProvider.AzureOpenAI;

    /// <summary>When set, every live answer is also written here so later runs and tests can replay it.</summary>
    public string? RecordTo { get; init; }

    /// <summary>The directory <see cref="ExtractionProvider.Replay"/> reads answers from.</summary>
    public string RecordingsPath { get; init; } = "recordings";

    /// <summary>
    /// When true, a live run reuses the recorded answer from <see cref="RecordingsPath"/> for any
    /// page whose text is unchanged, and calls the model only for pages that changed.
    /// </summary>
    public bool ReuseUnchangedPages { get; init; } = true;

    [Required]
    public AzureOpenAiOptions AzureOpenAI { get; init; } = new();

    [Required]
    public AnthropicOptions Anthropic { get; init; } = new();
}
