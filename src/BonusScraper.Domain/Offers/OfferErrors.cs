using ErrorOr;

namespace BonusScraper.Domain.Offers;

public static class OfferErrors
{
    public static readonly Error CasinoNameMissing =
        Error.Validation("Offer.CasinoNameMissing", "A casino name is required.");

    public static readonly Error CasinoNameTooLong =
        Error.Validation("Offer.CasinoNameTooLong", $"A casino name cannot exceed {CasinoName.MaxLength} characters.");

    public static readonly Error SourceUrlNotAbsolute =
        Error.Validation("Offer.SourceUrlNotAbsolute", "The source URL must be absolute.");

    public static readonly Error SourceUrlNotHttp =
        Error.Validation("Offer.SourceUrlNotHttp", "The source URL must use http or https.");

    public static readonly Error WageringNegative =
        Error.Validation("Offer.WageringNegative", "A wagering multiplier cannot be negative.");

    public static readonly Error WelcomeBonusSummaryMissing =
        Error.Validation("Offer.WelcomeBonusSummaryMissing", "A welcome bonus summary is required.");

    public static readonly Error WelcomeBonusSummaryTooLong =
        Error.Validation("Offer.WelcomeBonusSummaryTooLong", $"A welcome bonus summary cannot exceed {WelcomeBonus.MaxSummaryLength} characters.");

    public static readonly Error FreeSpinsNegative =
        Error.Validation("Offer.FreeSpinsNegative", "Free spins cannot be negative.");

    public static readonly Error RawTermsMissing =
        Error.Validation("Offer.RawTermsMissing", "The raw terms the offer was extracted from must be kept.");

    public static Error WageringImplausible(decimal multiplier) =>
        Error.Validation(
            "Offer.WageringImplausible",
            $"A wagering multiplier of {multiplier} exceeds the plausible maximum of {WageringRequirement.MaxPlausibleMultiplier}.");

    public static Error MatchPercentOutOfRange(int percent) =>
        Error.Validation(
            "Offer.MatchPercentOutOfRange",
            $"A match percentage of {percent} is outside 1 to {WelcomeBonus.MaxMatchPercent}.");
}
