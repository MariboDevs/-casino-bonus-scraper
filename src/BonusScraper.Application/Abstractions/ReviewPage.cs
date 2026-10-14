namespace BonusScraper.Application.Abstractions;

/// <summary>
/// The bonus-relevant text of one review page, already stripped of markup. Each part
/// is optional because sources are inconsistent: some pages carry a structured offer
/// box, others only describe the offer in the article body.
/// </summary>
public sealed record ReviewPage(Uri Source, string? Headline, string? Terms, string? Body, DateTimeOffset FetchedAt)
{
    public bool HasBonusText =>
        !string.IsNullOrWhiteSpace(Headline) || !string.IsNullOrWhiteSpace(Terms) || !string.IsNullOrWhiteSpace(Body);

    /// <summary>The exact text the extractor reads and the cross-check verifies against.</summary>
    public string CombinedText => string.Join(
        "\n\n",
        new[] { Headline, Terms, Body }.Where(part => !string.IsNullOrWhiteSpace(part)));
}
