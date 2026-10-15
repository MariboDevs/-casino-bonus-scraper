using BonusScraper.Domain.Monetary;
using BonusScraper.Domain.Verification;
using ErrorOr;

namespace BonusScraper.Domain.Offers;

/// <summary>
/// One casino's welcome offer as read from one source on one run. The source text is
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
        OfferText text,
        string extractedBy,
        DateTimeOffset extractedAt,
        bool answerReused,
        OfferVerification verification)
    {
        Casino = casino;
        WelcomeBonus = welcomeBonus;
        Wagering = wagering;
        MinimumDeposit = minimumDeposit;
        Source = source;
        Text = text;
        ExtractedBy = extractedBy;
        ExtractedAt = extractedAt;
        AnswerReused = answerReused;
        Verification = verification;
    }

    public CasinoName Casino { get; }

    public WelcomeBonus WelcomeBonus { get; }

    public WageringRequirement? Wagering { get; }

    public Money? MinimumDeposit { get; }

    public SourceUrl Source { get; }

    public OfferText Text { get; }

    public string ExtractedBy { get; }

    public DateTimeOffset ExtractedAt { get; }

    /// <summary>
    /// True when the model's answer was given on an earlier run for the identical page text and
    /// reused. <see cref="ExtractedAt"/> is then when the page was fetched on this run.
    /// </summary>
    public bool AnswerReused { get; }

    public OfferVerification Verification { get; }

    public static ErrorOr<BonusOffer> Create(
        CasinoName casino,
        WelcomeBonus welcomeBonus,
        WageringRequirement? wagering,
        Money? minimumDeposit,
        SourceUrl source,
        OfferText text,
        string extractedBy,
        DateTimeOffset extractedAt,
        bool answerReused = false)
    {
        var verification = TermsCrossCheck.Check(
            new ExtractedFigures(wagering?.Multiplier, minimumDeposit, welcomeBonus.MaxBonus, welcomeBonus.MatchPercent),
            text);

        return new BonusOffer(casino, welcomeBonus, wagering, minimumDeposit, source, text, extractedBy, extractedAt, answerReused, verification);
    }
}
