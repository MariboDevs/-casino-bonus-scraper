using BonusScraper.Domain.Monetary;

namespace BonusScraper.Domain.Verification;

/// <summary>What a deterministic regular-expression reading of the raw terms found.</summary>
public sealed record TermsFigures(
    IReadOnlyList<decimal> WageringMultipliers,
    IReadOnlyList<Money> MinimumDeposits,
    IReadOnlyList<Money> MaxBonuses,
    IReadOnlyList<int> MatchPercents);
