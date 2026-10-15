namespace BonusScraper.Application.Abstractions;

/// <summary>
/// The language model's structured reading of free-text bonus terms. Every figure is
/// nullable because "the terms do not say" is a valid answer and must not be guessed.
/// <see cref="Reused"/> is true when the answer was given on an earlier run for the identical
/// page text and reused rather than asked for again.
/// </summary>
public sealed record ExtractedBonusTerms(
    string WelcomeBonusSummary,
    int? MatchPercent,
    decimal? MaxBonusAmount,
    int? FreeSpins,
    decimal? WageringMultiplier,
    decimal? MinimumDepositAmount,
    string? Currency,
    string Model,
    bool Reused = false);
