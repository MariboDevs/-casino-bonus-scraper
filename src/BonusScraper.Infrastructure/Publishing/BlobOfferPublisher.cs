using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BonusScraper.Application.Abstractions;
using BonusScraper.Domain.Runs;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace BonusScraper.Infrastructure.Publishing;

/// <summary>
/// Uploads the offers JSON to Blob storage with a managed identity. The container has
/// versioning on, so each day's file is kept and a bad run can be rolled back.
/// </summary>
internal sealed class BlobOfferPublisher(IOptions<PublishingOptions> options) : IOfferPublisher
{
    public const string BlobName = "casinos.json";

    /// <summary>Short enough that a new run shows within minutes, long enough to absorb traffic.</summary>
    private const string CacheControl = "public, max-age=300";

    public async Task<ErrorOr<string>> PublishAsync(ScrapeRun run, CancellationToken cancellationToken)
    {
        var container = new BlobContainerClient(options.Value.BlobContainerUrl!, new DefaultAzureCredential());
        var blob = container.GetBlobClient(BlobName);

        using var buffer = new MemoryStream();
        await PublishedOffersSerializer.SerializeAsync(buffer, run, options.Value.SourceName, cancellationToken);
        buffer.Position = 0;

        try
        {
            await blob.UploadAsync(
                buffer,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = "application/json; charset=utf-8", CacheControl = CacheControl },
                },
                cancellationToken);
            return blob.Uri.AbsoluteUri;
        }
        catch (RequestFailedException ex)
        {
            return Error.Unexpected("Publish.UploadFailed", $"The offers blob could not be uploaded ({ex.Status} {ex.ErrorCode}).");
        }
        catch (AuthenticationFailedException)
        {
            return Error.Unexpected("Publish.UploadFailed", "No identity could authenticate to Blob storage.");
        }
    }
}
