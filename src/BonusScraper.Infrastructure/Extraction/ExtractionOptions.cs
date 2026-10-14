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

    [Required]
    public AzureOpenAiOptions AzureOpenAI { get; init; } = new();

    [Required]
    public AnthropicOptions Anthropic { get; init; } = new();
}
