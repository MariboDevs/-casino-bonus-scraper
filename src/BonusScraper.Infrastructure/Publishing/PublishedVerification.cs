namespace BonusScraper.Infrastructure.Publishing;

/// <summary><paramref name="Basis"/> says which text the check read: "operatorTerms" or "reviewArticle".</summary>
internal sealed record PublishedVerification(string Status, string Basis, IReadOnlyList<string> Notes);
