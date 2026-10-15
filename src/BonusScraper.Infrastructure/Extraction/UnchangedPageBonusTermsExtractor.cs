using BonusScraper.Application.Abstractions;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BonusScraper.Infrastructure.Extraction;

/// <summary>
/// Change detection: asks the live model only when a page has changed. A recording is keyed by
/// a hash of the prompt version, the casino and the exact page text, so a hit means the model
/// would be asked the identical question it already answered, and the recorded answer is
/// reused at no cost. A changed page, a changed prompt or an unreadable recording goes to the
/// live model. The cross-check still runs on every answer, reused or not.
/// </summary>
internal sealed class UnchangedPageBonusTermsExtractor(
    IBonusTermsExtractor recorded,
    IBonusTermsExtractor live,
    ILogger<UnchangedPageBonusTermsExtractor> logger) : IBonusTermsExtractor
{
    public async Task<ErrorOr<ExtractedBonusTerms>> ExtractAsync(BonusTermsDocument document, CancellationToken cancellationToken)
    {
        var reused = await recorded.ExtractAsync(document, cancellationToken);
        if (!reused.IsError)
        {
            logger.LogInformation("Page unchanged for {Casino}; reused the recorded answer", document.Casino);
            return reused;
        }

        return await live.ExtractAsync(document, cancellationToken);
    }
}
