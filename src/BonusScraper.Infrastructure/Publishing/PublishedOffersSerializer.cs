using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using BonusScraper.Domain.Monetary;
using BonusScraper.Domain.Offers;
using BonusScraper.Domain.Runs;

namespace BonusScraper.Infrastructure.Publishing;

/// <summary>Maps a run onto the published contract and serialises it. Shared by every publisher.</summary>
internal static class PublishedOffersSerializer
{
    public const int SchemaVersion = 1;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static Task SerializeAsync(Stream stream, ScrapeRun run, string sourceName, CancellationToken cancellationToken) =>
        JsonSerializer.SerializeAsync(stream, ToDocument(run, sourceName), Json, cancellationToken);

    private static PublishedOffers ToDocument(ScrapeRun run, string sourceName) => new(
        SchemaVersion,
        run.CompletedAt ?? run.StartedAt,
        run.Id,
        sourceName,
        JsonNamingPolicy.CamelCase.ConvertName(run.Outcome.ToString()),
        run.Offers.OrderBy(o => o.Casino.Value, StringComparer.OrdinalIgnoreCase).Select(ToPublished).ToList(),
        run.Failures.Select(f => new PublishedFailure(f.Casino, f.Source.AbsoluteUri, f.Code)).ToList());

    private static PublishedOffer ToPublished(BonusOffer offer) => new(
        offer.Casino.Value,
        offer.WelcomeBonus.Summary,
        offer.Wagering?.Multiplier,
        offer.MinimumDeposit?.ToString(),
        offer.Source.ToString(),
        new PublishedOfferDetails(
            offer.WelcomeBonus.MatchPercent,
            ToMoney(offer.WelcomeBonus.MaxBonus),
            offer.WelcomeBonus.FreeSpins,
            ToMoney(offer.MinimumDeposit)),
        new PublishedVerification(
            JsonNamingPolicy.CamelCase.ConvertName(offer.Verification.Status.ToString()),
            offer.Verification.Notes.ToList()),
        offer.ExtractedBy,
        offer.ExtractedAt);

    private static PublishedMoney? ToMoney(Money? money) =>
        money is null ? null : new PublishedMoney(money.Amount, money.Currency.Code);
}
