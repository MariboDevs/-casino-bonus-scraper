using System.Text.Json;
using BonusScraper.Application.Abstractions;
using ErrorOr;

namespace BonusScraper.Infrastructure.Extraction;

internal static class BonusTermsResponseParser
{
    public static ErrorOr<ExtractedBonusTerms> Parse(string? json, string model)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return ExtractorErrors.EmptyResponse;
        }

        BonusTermsResponse? response;
        try
        {
            response = JsonSerializer.Deserialize<BonusTermsResponse>(json);
        }
        catch (JsonException)
        {
            return ExtractorErrors.MalformedResponse;
        }

        if (response?.WelcomeBonusSummary is not { Length: > 0 } summary)
        {
            return ExtractorErrors.MalformedResponse;
        }

        return new ExtractedBonusTerms(
            summary,
            response.MatchPercent,
            response.MaxBonusAmount,
            response.FreeSpins,
            response.WageringMultiplier,
            response.MinimumDepositAmount,
            response.Currency,
            model);
    }
}
