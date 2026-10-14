namespace BonusScraper.Application.Abstractions;

/// <summary>
/// The language model's structured reading of free-text bonus terms. Every figure is
/// nullable because "the terms do not say" is a valid answer and must not be guessed.
/// </summary>
public sealed record ExtractedBonusTerms(
    string WelcomeBonusSummary,
    int? MatchPercent,
    decimal? MaxBonusAmount,
    int? FreeSpins,
    decimal? WageringMultiplier,
    decimal? MinimumDepositAmount,
    string? Currency,
    string Model);
