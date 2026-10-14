using BonusScraper.Application.Abstractions;
using BonusScraper.Domain.Runs;
using BonusScraper.Domain.Verification;
using ErrorOr;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BonusScraper.Application.Scraping;

/// <summary>
/// Scrapes each target in turn. One casino failing never fails the others: its failure
/// is recorded on the run and the run carries on. Only a run that produced nothing at
/// all is an error, and that case deliberately leaves the last published data in place.
/// </summary>
public sealed partial class ScrapeCasinosCommandHandler(
    IReviewPageSource pages,
    IBonusTermsExtractor extractor,
    IScrapeRunRepository runs,
    IOfferPublisher publisher,
    TimeProvider clock,
    ILogger<ScrapeCasinosCommandHandler> logger)
    : ICommandHandler<ScrapeCasinosCommand, ErrorOr<ScrapeRunSummary>>
{
    public async ValueTask<ErrorOr<ScrapeRunSummary>> Handle(ScrapeCasinosCommand command, CancellationToken cancellationToken)
    {
        var run = new ScrapeRun(Guid.CreateVersion7(), clock.GetUtcNow());

        foreach (var target in command.Targets)
        {
            var outcome = await ScrapeAsync(target, cancellationToken);
            outcome.Switch(
                offer =>
                {
                    run.Record(offer);
                    LogOfferRecorded(target.Name, offer.Verification.Status);
                },
                errors =>
                {
                    run.Record(new ScrapeFailure(target.Name, target.ReviewUrl, errors[0].Code, errors[0].Description));
                    LogCasinoFailed(target.Name, errors[0].Code);
                });
        }

        run.Complete(clock.GetUtcNow());

        var saved = await runs.SaveAsync(run, cancellationToken);
        if (saved.IsError)
        {
            return saved.Errors;
        }

        if (run.Outcome == ScrapeRunOutcome.Failed)
        {
            return ScrapeErrors.NothingScraped;
        }

        return (await publisher.PublishAsync(run, cancellationToken)).Then(location => new ScrapeRunSummary(
            run.Id,
            run.Outcome,
            run.Offers.Count,
            run.Offers.Count(o => o.Verification.Status == VerificationStatus.Provisional),
            run.Failures,
            location));
    }

    private async Task<ErrorOr<Domain.Offers.BonusOffer>> ScrapeAsync(CasinoTarget target, CancellationToken cancellationToken)
    {
        var page = await pages.ReadAsync(target.ReviewUrl, cancellationToken);
        if (page.IsError)
        {
            return page.Errors;
        }

        if (!page.Value.HasBonusText)
        {
            return ScrapeErrors.NoBonusText;
        }

        var extracted = await extractor.ExtractAsync(
            new BonusTermsDocument(target.Name, page.Value.CombinedText),
            cancellationToken);

        return extracted.Then(terms => OfferFactory.Create(target, page.Value, terms));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Recorded offer for {Casino} ({Verification})")]
    private partial void LogOfferRecorded(string casino, VerificationStatus verification);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No offer for {Casino}: {ErrorCode}")]
    private partial void LogCasinoFailed(string casino, string errorCode);
}
