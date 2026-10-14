namespace BonusScraper.Infrastructure.Sources;

internal sealed record ParsedReviewPage(string? Headline, string? Terms, string? Body)
{
    public bool IsEmpty => Headline is null && Terms is null && Body is null;
}
