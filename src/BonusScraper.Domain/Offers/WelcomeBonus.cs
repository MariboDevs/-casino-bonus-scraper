using BonusScraper.Domain.Monetary;
using ErrorOr;

namespace BonusScraper.Domain.Offers;

/// <summary>
/// The headline welcome offer. <see cref="Summary"/> is the human-readable form shown
/// in the table ("100% up to 200 GBP"); the structured parts are kept alongside it so
/// that downstream consumers can sort and filter without re-parsing prose.
/// </summary>
public sealed record WelcomeBonus
{
    public const int MaxSummaryLength = 200;
    public const int MaxMatchPercent = 1000;

    private WelcomeBonus(string summary, int? matchPercent, Money? maxBonus, int? freeSpins)
    {
        Summary = summary;
        MatchPercent = matchPercent;
        MaxBonus = maxBonus;
        FreeSpins = freeSpins;
    }

    public string Summary { get; }

    public int? MatchPercent { get; }

    public Money? MaxBonus { get; }

    public int? FreeSpins { get; }

    public static ErrorOr<WelcomeBonus> Create(string? summary, int? matchPercent, Money? maxBonus, int? freeSpins)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return OfferErrors.WelcomeBonusSummaryMissing;
        }

        if (summary.Length > MaxSummaryLength)
        {
            return OfferErrors.WelcomeBonusSummaryTooLong;
        }

        if (matchPercent is <= 0 or > MaxMatchPercent)
        {
            return OfferErrors.MatchPercentOutOfRange(matchPercent.Value);
        }

        if (freeSpins is < 0)
        {
            return OfferErrors.FreeSpinsNegative;
        }

        return new WelcomeBonus(summary.Trim(), matchPercent, maxBonus, freeSpins);
    }
}
