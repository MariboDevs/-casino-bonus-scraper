namespace BonusScraper.Infrastructure.Publishing;

internal sealed record PublishedVerification(string Status, IReadOnlyList<string> Notes);
