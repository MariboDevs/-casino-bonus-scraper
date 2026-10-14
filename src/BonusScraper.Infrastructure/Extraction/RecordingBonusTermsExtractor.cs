using System.Text.Json;
using BonusScraper.Application.Abstractions;
using ErrorOr;

namespace BonusScraper.Infrastructure.Extraction;

/// <summary>Decorates a live extractor, writing each successful answer to disk for <see cref="ReplayBonusTermsExtractor"/>.</summary>
internal sealed class RecordingBonusTermsExtractor(IBonusTermsExtractor inner, string directory) : IBonusTermsExtractor
{
    public async Task<ErrorOr<ExtractedBonusTerms>> ExtractAsync(BonusTermsDocument document, CancellationToken cancellationToken)
    {
        var result = await inner.ExtractAsync(document, cancellationToken);
        if (result.IsError)
        {
            return result;
        }

        Directory.CreateDirectory(directory);
        var recording = new Recording(Recording.KeyFor(document), document.Casino, BonusTermsPrompt.Version, result.Value);
        await using var stream = File.Create(Path.Combine(directory, Recording.FileName(document)));
        await JsonSerializer.SerializeAsync(stream, recording, RecordingJson.Options, cancellationToken);
        return result;
    }
}
