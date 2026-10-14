using System.Text.Json.Serialization;

namespace BonusScraper.Infrastructure.Extraction;

/// <summary>The wire shape of the model's answer, mirroring <see cref="BonusTermsSchema"/>.</summary>
internal sealed record BonusTermsResponse(
    [property: JsonPropertyName("welcome_bonus_summary")] string? WelcomeBonusSummary,
    [property: JsonPropertyName("match_percent")] int? MatchPercent,
    [property: JsonPropertyName("max_bonus_amount")] decimal? MaxBonusAmount,
    [property: JsonPropertyName("free_spins")] int? FreeSpins,
    [property: JsonPropertyName("wagering_multiplier")] decimal? WageringMultiplier,
    [property: JsonPropertyName("minimum_deposit_amount")] decimal? MinimumDepositAmount,
    [property: JsonPropertyName("currency")] string? Currency);
