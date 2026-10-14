namespace BonusScraper.Infrastructure.Publishing;

internal sealed record PublishedOfferDetails(
    int? MatchPercent,
    PublishedMoney? MaxBonus,
    int? FreeSpins,
    PublishedMoney? MinimumDeposit);
