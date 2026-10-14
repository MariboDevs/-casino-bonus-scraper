namespace BonusScraper.Application.Scraping;

/// <summary>A casino to scrape and the review page its offer is read from.</summary>
public sealed record CasinoTarget(string Name, Uri ReviewUrl);
