using BonusScraper.Application.Abstractions;
using BonusScraper.Domain.Monetary;
using BonusScraper.Domain.Offers;
using ErrorOr;

namespace BonusScraper.Application.Scraping;

/// <summary>Maps the model's loosely-typed reading onto validated domain types.</summary>
internal static class OfferFactory
{
    public static ErrorOr<BonusOffer> Create(CasinoTarget target, ReviewPage page, ExtractedBonusTerms terms)
    {
        var errors = new List<Error>();

        var casino = Collect(CasinoName.Create(target.Name), errors);
        var source = Collect(SourceUrl.Create(page.Source), errors);
        var maxBonus = OptionalMoney(terms.MaxBonusAmount, terms.Currency, errors);
        var minimumDeposit = OptionalMoney(terms.MinimumDepositAmount, terms.Currency, errors);
        var wagering = terms.WageringMultiplier is { } multiplier
            ? Collect(WageringRequirement.Create(multiplier), errors)
            : null;
        var text = Collect(OfferText.Create(page.OperatorText, page.Body), errors);
        var welcome = Collect(WelcomeBonus.Create(terms.WelcomeBonusSummary, terms.MatchPercent, maxBonus, terms.FreeSpins), errors);

        if (errors.Count > 0)
        {
            return errors;
        }

        return BonusOffer.Create(
            casino!,
            welcome!,
            wagering,
            minimumDeposit,
            source!,
            text!,
            terms.Model,
            page.FetchedAt,
            terms.Reused);
    }

    private static Money? OptionalMoney(decimal? amount, string? currency, List<Error> errors) =>
        amount is { } value ? Collect(Money.Create(value, currency), errors) : null;

    private static T? Collect<T>(ErrorOr<T> result, List<Error> errors)
        where T : class
    {
        if (result.IsError)
        {
            errors.AddRange(result.Errors);
            return null;
        }

        return result.Value;
    }
}
