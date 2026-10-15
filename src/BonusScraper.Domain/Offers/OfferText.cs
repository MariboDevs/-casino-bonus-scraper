using BonusScraper.Domain.Verification;
using ErrorOr;

namespace BonusScraper.Domain.Offers;

/// <summary>
/// The text an offer was read from, split by authority. A review's article prose is often
/// older than the operator's terms box beside it (one review here still describes last
/// year's offer), so the operator's terms are what an offer is verified against whenever
/// the page has them, and the article only when it does not.
/// </summary>
public sealed record OfferText
{
    private OfferText(string? operatorTerms, string? reviewArticle)
    {
        OperatorTerms = operatorTerms;
        ReviewArticle = reviewArticle;
    }

    public string? OperatorTerms { get; }

    public string? ReviewArticle { get; }

    public VerificationBasis Basis => OperatorTerms is null ? VerificationBasis.ReviewArticle : VerificationBasis.OperatorTerms;

    /// <summary>The text that decides: the operator's terms when present, otherwise the article.</summary>
    public string Authoritative => OperatorTerms ?? ReviewArticle!;

    /// <summary>Everything that was read, in page order, as stored and as given to the model.</summary>
    public string Full => string.Join("\n\n", new[] { OperatorTerms, ReviewArticle }.Where(t => t is not null));

    public static ErrorOr<OfferText> Create(string? operatorTerms, string? reviewArticle)
    {
        var terms = string.IsNullOrWhiteSpace(operatorTerms) ? null : operatorTerms;
        var article = string.IsNullOrWhiteSpace(reviewArticle) ? null : reviewArticle;

        return terms is null && article is null ? OfferErrors.RawTermsMissing : new OfferText(terms, article);
    }
}
