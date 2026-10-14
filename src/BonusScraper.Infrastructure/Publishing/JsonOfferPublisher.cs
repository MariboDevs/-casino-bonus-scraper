using BonusScraper.Application.Abstractions;
using BonusScraper.Domain.Runs;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace BonusScraper.Infrastructure.Publishing;

/// <summary>
/// Writes the run's offers to the JSON file the page is generated from. The file is
/// written to a temporary sibling and moved into place, so a reader (or a crash) never
/// sees half a file.
/// </summary>
internal sealed class JsonOfferPublisher(IOptions<PublishingOptions> options) : IOfferPublisher
{
    public async Task<ErrorOr<string>> PublishAsync(ScrapeRun run, CancellationToken cancellationToken)
    {
        var path = Path.GetFullPath(options.Value.OutputPath);
        var temporary = path + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await using (var stream = File.Create(temporary))
            {
                await PublishedOffersSerializer.SerializeAsync(stream, run, options.Value.SourceName, cancellationToken);
            }

            File.Move(temporary, path, overwrite: true);
            return options.Value.OutputPath;
        }
        catch (IOException ex)
        {
            return Error.Unexpected("Publish.WriteFailed", $"The offers file could not be written: {ex.GetType().Name}.");
        }
        catch (UnauthorizedAccessException)
        {
            return Error.Unexpected("Publish.WriteFailed", "The offers file could not be written: access denied.");
        }
    }
}
