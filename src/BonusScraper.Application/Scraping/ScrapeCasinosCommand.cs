using ErrorOr;
using Mediator;

namespace BonusScraper.Application.Scraping;

public sealed record ScrapeCasinosCommand(IReadOnlyList<CasinoTarget> Targets) : ICommand<ErrorOr<ScrapeRunSummary>>;
