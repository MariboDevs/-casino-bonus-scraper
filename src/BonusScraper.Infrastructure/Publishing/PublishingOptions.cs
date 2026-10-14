using System.ComponentModel.DataAnnotations;

namespace BonusScraper.Infrastructure.Publishing;

public sealed class PublishingOptions
{
    public const string SectionName = "Publishing";

    /// <summary>The JSON file the web page is built from. Relative paths resolve from the working directory.</summary>
    [Required]
    public string OutputPath { get; init; } = "data/casinos.json";

    /// <summary>
    /// Optional. When set, the same JSON is also uploaded to this Blob container (with a
    /// managed identity), which is where the production page reads it from.
    /// </summary>
    public Uri? BlobContainerUrl { get; init; }

    /// <summary>Shown on the page so a reader knows where the numbers came from.</summary>
    [Required]
    public string SourceName { get; init; } = "casinoguide.co.uk";
}
