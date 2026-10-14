using BonusScraper.Domain.Monetary;
using BonusScraper.Domain.Verification;
using ErrorOr;

namespace BonusScraper.Domain.Offers;

/// <summary>
/// One casino's welcome offer as read from one source on one run. The raw terms are
/// part of the aggregate, not a log line: whatever the model concluded, the text it
/// concluded it from travels with the result.
/// </summary>
public sealed class BonusOffer
{
    private BonusOffer(
        CasinoName casino,
        WelcomeBonus welcomeBonus,
        WageringRequirement? wagering,
        Money? minimumDeposit,
        SourceUrl source,
        string rawTerms,
        string extractedBy,
        DateTimeOffset extractedAt,
        OfferVerification verification)
    {
        Casino = casino;
        WelcomeBonus = welcomeBonus;
        Wagering = wagering;
        MinimumDeposit = minimumDeposit;
        Source = source;
        RawTerms = rawTerms;
        ExtractedBy = extractedBy;
        ExtractedAt = extractedAt;
        Verification = verification;
    }

    public CasinoName Casino { get; }

    public WelcomeBonus WelcomeBonus { get; }

    public WageringRequirement? Wagering { get; }

    public Money? MinimumDeposit { get; }

    public SourceUrl Source { get; }

    public string RawTerms { get; }

    public string ExtractedBy { get; }

    public DateTimeOffset ExtractedAt { get; }

    public OfferVerification Verification { get; }

    public static ErrorOr<BonusOffer> Create(
        CasinoName casino,
        WelcomeBonus welcomeBonus,
        WageringRequirement? wagering,
        Money? minimumDeposit,
        SourceUrl source,
        string? rawTerms,
        string extractedBy,
        DateTimeOffset extractedAt)
    {
        if (string.IsNullOrWhiteSpace(rawTerms))
        {
            return OfferErrors.RawTermsMissing;
        }

        var verification = TermsCrossCheck.Check(
            new ExtractedFigures(wagering?.Multiplier, minimumDeposit, welcomeBonus.MaxBonus, welcomeBonus.MatchPercent),
            rawTerms);

        return new BonusOffer(casino, welcomeBonus, wagering, minimumDeposit, source, rawTerms, extractedBy, extractedAt, verification);
    }
}
