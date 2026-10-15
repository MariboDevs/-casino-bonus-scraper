using BonusScraper.Domain.Monetary;
using BonusScraper.Domain.Offers;
using BonusScraper.Domain.Verification;

namespace BonusScraper.Domain.UnitTests.Verification;

public sealed class TermsCrossCheckTests
{
    private const string Terms = "Min £10 first deposit. 100% bonus up to £200. 40x wagering applies.";

    [Fact]
    public void Confirms_an_offer_whose_every_figure_is_in_the_terms()
    {
        var result = TermsCrossCheck.Check(new ExtractedFigures(40, Gbp(10), Gbp(200), 100), Operator(Terms));

        result.Status.ShouldBe(VerificationStatus.Confirmed);
        result.Checks.ShouldAllBe(c => c.Outcome == FieldCheckOutcome.Corroborated);
        result.Notes.ShouldBeEmpty();
    }

    [Fact]
    public void Flags_a_figure_the_terms_contradict()
    {
        var result = TermsCrossCheck.Check(new ExtractedFigures(35, Gbp(10), Gbp(200), 100), Operator(Terms));

        result.Status.ShouldBe(VerificationStatus.Provisional);
        var wagering = result.Checks.Single(c => c.Field == OfferField.WageringMultiplier);
        wagering.Outcome.ShouldBe(FieldCheckOutcome.Contradicted);
        wagering.Describe().ShouldBe("Wagering: the model read 35x, but the terms say 40x.");
    }

    [Fact]
    public void Flags_a_figure_the_model_returned_but_the_terms_never_state()
    {
        var result = TermsCrossCheck.Check(new ExtractedFigures(40, Gbp(10), Gbp(200), 100), Operator("40x wagering. 100% bonus up to £200."));

        result.Status.ShouldBe(VerificationStatus.Provisional);
        var deposit = result.Checks.Single(c => c.Field == OfferField.MinimumDeposit);
        deposit.Outcome.ShouldBe(FieldCheckOutcome.Unverifiable);
        deposit.Describe().ShouldBe("Minimum deposit: the model read 10 GBP, which the terms do not state.");
    }

    [Fact]
    public void Flags_a_figure_the_model_missed()
    {
        var result = TermsCrossCheck.Check(new ExtractedFigures(null, Gbp(10), Gbp(200), 100), Operator(Terms));

        result.Status.ShouldBe(VerificationStatus.Provisional);
        var wagering = result.Checks.Single(c => c.Field == OfferField.WageringMultiplier);
        wagering.Outcome.ShouldBe(FieldCheckOutcome.Contradicted);
        wagering.Extracted.ShouldBe("nothing");
        wagering.Describe().ShouldBe("Wagering: the terms say 40x, but the model did not report one.");
    }

    [Fact]
    public void Treats_a_field_neither_side_has_as_absent_not_blocking()
    {
        var result = TermsCrossCheck.Check(new ExtractedFigures(40, Gbp(10), Gbp(200), null), Operator("Min £10 deposit. Get £200. 40x wagering."));

        result.Status.ShouldBe(VerificationStatus.Confirmed);
        result.Checks.Single(c => c.Field == OfferField.MatchPercent).Outcome.ShouldBe(FieldCheckOutcome.Absent);
    }

    [Fact]
    public void Treats_a_currency_mismatch_as_a_contradiction()
    {
        var result = TermsCrossCheck.Check(new ExtractedFigures(40, Money.Create(10, "EUR").Value, Gbp(200), 100), Operator(Terms));

        result.Checks.Single(c => c.Field == OfferField.MinimumDeposit).Outcome.ShouldBe(FieldCheckOutcome.Contradicted);
    }

    [Fact]
    public void Lets_the_operator_terms_overrule_stale_article_prose()
    {
        // Grosvenor on 2026-10-15: the article still describes an older 100% match offer.
        var text = OfferText.Create(
            "Deposit £20 Get £30\n\n18+ New customers only. Min deposit £20, PayPal & Paysafe excluded. £30 bonus (x30 wagering) on selected games, Max wins £2K.",
            "Sign up with Grosvenor Casino today and deposit just £20 to claim your a 100% match bonus of up to £20 and Grosvenor will throw in a £10 bonus.").Value;

        var result = TermsCrossCheck.Check(new ExtractedFigures(30, Gbp(20), Gbp(30), null), text);

        result.Status.ShouldBe(VerificationStatus.Confirmed);
        result.Basis.ShouldBe(VerificationBasis.OperatorTerms);
    }

    [Fact]
    public void Falls_back_to_the_article_when_the_page_has_no_operator_terms()
    {
        var text = OfferText.Create(null, "Remember the minimum you can deposit is 100 euros. Wagering 30x.").Value;

        var result = TermsCrossCheck.Check(new ExtractedFigures(30, Money.Create(100, "EUR").Value, null, null), text);

        result.Status.ShouldBe(VerificationStatus.Confirmed);
        result.Basis.ShouldBe(VerificationBasis.ReviewArticle);
    }

    private static OfferText Operator(string terms) => OfferText.Create(terms, null).Value;

    private static Money Gbp(decimal amount) => Money.Create(amount, "GBP").Value;
}
