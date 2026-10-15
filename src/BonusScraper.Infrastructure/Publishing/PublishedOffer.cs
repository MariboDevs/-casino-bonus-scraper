namespace BonusScraper.Infrastructure.Publishing;

internal sealed record PublishedOffer(
    string Casino,
    string WelcomeBonus,
    decimal? WageringRequirement,
    string? MinimumDeposit,
    string SourceUrl,
    PublishedOfferDetails Details,
    PublishedVerification Verification,
    string ExtractedBy,
    DateTimeOffset ExtractedAt,
    bool AnswerReused);
