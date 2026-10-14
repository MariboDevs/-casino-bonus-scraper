namespace BonusScraper.Infrastructure.Publishing;

/// <summary>
/// The published JSON contract the web page reads. The four fields the page shows are
/// flat strings and numbers; the structured detail sits alongside for anyone who wants
/// to sort or filter. Changing a name here is a breaking change for the page.
/// </summary>
internal sealed record PublishedOffers(
    int SchemaVersion,
    DateTimeOffset GeneratedAt,
    Guid RunId,
    string Source,
    string Outcome,
    IReadOnlyList<PublishedOffer> Casinos,
    IReadOnlyList<PublishedFailure> Failures);
