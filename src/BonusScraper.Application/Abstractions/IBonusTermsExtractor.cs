using ErrorOr;

namespace BonusScraper.Application.Abstractions;

/// <summary>Turns free-text bonus terms into structured fields using a language model.</summary>
public interface IBonusTermsExtractor
{
    Task<ErrorOr<ExtractedBonusTerms>> ExtractAsync(BonusTermsDocument document, CancellationToken cancellationToken);
}
