using BonusScraper.Domain.Monetary;

namespace BonusScraper.Domain.Verification;

/// <summary>The language model's figures, in the shape the cross-check compares.</summary>
public sealed record ExtractedFigures(decimal? WageringMultiplier, Money? MinimumDeposit, Money? MaxBonus, int? MatchPercent);
