using ErrorOr;

namespace BonusScraper.Application.Scraping;

public static class ScrapeErrors
{
    public static readonly Error NothingScraped = Error.Failure(
        "Scrape.NothingScraped",
        "No casino produced an offer, so the previously published data was left untouched.");

    public static readonly Error NoBonusText = Error.NotFound(
        "Scrape.NoBonusText",
        "The page was read but contained no recognisable bonus text.");
}
