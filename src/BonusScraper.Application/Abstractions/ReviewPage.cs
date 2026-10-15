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

    /// <summary>The operator's own words: the offer box headline and its significant terms.</summary>
    public string? OperatorText => Join(Headline, Terms);

    /// <summary>Everything bonus-related on the page, in page order: exactly what the model reads.</summary>
    public string CombinedText => Join(Headline, Terms, Body) ?? string.Empty;

    private static string? Join(params string?[] parts)
    {
        var present = parts.Where(part => !string.IsNullOrWhiteSpace(part)).ToArray();
        return present.Length == 0 ? null : string.Join("\n\n", present);
    }
}
