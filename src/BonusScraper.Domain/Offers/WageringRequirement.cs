using System.Globalization;
using ErrorOr;

namespace BonusScraper.Domain.Offers;

/// <summary>
/// How many times the bonus (or deposit plus bonus) must be played through before
/// winnings can be withdrawn. Zero is a legitimate "no wagering" offer.
/// </summary>
public sealed record WageringRequirement
{
    /// <summary>Above this the value is almost certainly a misread (a percentage or an amount), not a multiplier.</summary>
    public const decimal MaxPlausibleMultiplier = 200m;

    private WageringRequirement(decimal multiplier) => Multiplier = multiplier;

    public decimal Multiplier { get; }

    public bool IsWageringFree => Multiplier == 0m;

    public static ErrorOr<WageringRequirement> Create(decimal multiplier)
    {
        if (multiplier < 0m)
        {
            return OfferErrors.WageringNegative;
        }

        return multiplier > MaxPlausibleMultiplier
            ? OfferErrors.WageringImplausible(multiplier)
            : new WageringRequirement(multiplier);
    }

    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"{Multiplier:0.##}x");
}
