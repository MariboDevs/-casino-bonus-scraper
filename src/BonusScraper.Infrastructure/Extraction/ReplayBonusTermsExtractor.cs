using System.Text.Json;
using BonusScraper.Application.Abstractions;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace BonusScraper.Infrastructure.Extraction;

/// <summary>Serves answers recorded from a live model. Used by tests and by anyone running the scraper without a model key.</summary>
internal sealed class ReplayBonusTermsExtractor(IOptions<ExtractionOptions> options) : IBonusTermsExtractor
{
    public async Task<ErrorOr<ExtractedBonusTerms>> ExtractAsync(BonusTermsDocument document, CancellationToken cancellationToken)
    {
        var path = Path.Combine(options.Value.RecordingsPath, Recording.FileName(document));
        if (!File.Exists(path))
        {
            return ExtractorErrors.RecordingMissing;
        }

        await using var stream = File.OpenRead(path);
        var recording = await JsonSerializer.DeserializeAsync<Recording>(stream, RecordingJson.Options, cancellationToken);
        return recording is null ? ExtractorErrors.MalformedResponse : recording.Terms with { Reused = true };
    }
}
