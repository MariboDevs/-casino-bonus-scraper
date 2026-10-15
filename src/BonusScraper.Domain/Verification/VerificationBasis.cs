namespace BonusScraper.Domain.Verification;

/// <summary>Which text the cross-check verified an offer against.</summary>
public enum VerificationBasis
{
    /// <summary>The operator's own terms (the "18+ ..." significant terms and the offer headline).</summary>
    OperatorTerms,

    /// <summary>Only the review's article prose: used when the page carries no operator terms.</summary>
    ReviewArticle,
}
