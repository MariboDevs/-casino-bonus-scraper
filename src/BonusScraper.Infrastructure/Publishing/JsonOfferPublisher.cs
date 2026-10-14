using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using BonusScraper.Application.Abstractions;
using BonusScraper.Domain.Monetary;
using BonusScraper.Domain.Offers;
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
    public const int SchemaVersion = 1;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public async Task<ErrorOr<string>> PublishAsync(ScrapeRun run, CancellationToken cancellationToken)
    {
        var document = new PublishedOffers(
            SchemaVersion,
            run.CompletedAt ?? run.StartedAt,
            run.Id,
            options.Value.SourceName,
            JsonNamingPolicy.CamelCase.ConvertName(run.Outcome.ToString()),
            run.Offers.OrderBy(o => o.Casino.Value, StringComparer.OrdinalIgnoreCase).Select(ToPublished).ToList(),
            run.Failures.Select(f => new PublishedFailure(f.Casino, f.Source.AbsoluteUri, f.Code)).ToList());

        var path = Path.GetFullPath(options.Value.OutputPath);
        var temporary = path + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await using (var stream = File.Create(temporary))
            {
                await JsonSerializer.SerializeAsync(stream, document, Json, cancellationToken);
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

    private static PublishedOffer ToPublished(BonusOffer offer) => new(
        offer.Casino.Value,
        offer.WelcomeBonus.Summary,
        offer.Wagering?.Multiplier,
        offer.MinimumDeposit?.ToString(),
        offer.Source.ToString(),
        new PublishedOfferDetails(
            offer.WelcomeBonus.MatchPercent,
            Money(offer.WelcomeBonus.MaxBonus),
            offer.WelcomeBonus.FreeSpins,
            Money(offer.MinimumDeposit)),
        new PublishedVerification(
            JsonNamingPolicy.CamelCase.ConvertName(offer.Verification.Status.ToString()),
            offer.Verification.Notes.ToList()),
        offer.ExtractedBy,
        offer.ExtractedAt);

    private static PublishedMoney? Money(Money? money) =>
        money is null ? null : new PublishedMoney(money.Amount, money.Currency.Code);
}
