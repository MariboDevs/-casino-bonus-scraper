using BonusScraper.Domain.Monetary;
using BonusScraper.Domain.Verification;

namespace BonusScraper.Domain.UnitTests.Verification;

public sealed class TermsCrossCheckTests
{
    private const string Terms = "Min £10 first deposit. 100% bonus up to £200. 40x wagering applies.";

    [Fact]
    public void Confirms_an_offer_whose_every_figure_is_in_the_terms()
    {
        var result = TermsCrossCheck.Check(new ExtractedFigures(40, Gbp(10), Gbp(200), 100), Terms);

        result.Status.ShouldBe(VerificationStatus.Confirmed);
        result.Checks.ShouldAllBe(c => c.Outcome == FieldCheckOutcome.Corroborated);
        result.Notes.ShouldBeEmpty();
    }

    [Fact]
    public void Flags_a_figure_the_terms_contradict()
    {
        var result = TermsCrossCheck.Check(new ExtractedFigures(35, Gbp(10), Gbp(200), 100), Terms);

        result.Status.ShouldBe(VerificationStatus.Provisional);
        var wagering = result.Checks.Single(c => c.Field == OfferField.WageringMultiplier);
        wagering.Outcome.ShouldBe(FieldCheckOutcome.Contradicted);
        wagering.Describe().ShouldBe("WageringMultiplier: extracted 35x but the terms say 40x.");
    }

    [Fact]
    public void Flags_a_figure_the_model_returned_but_the_terms_never_state()
    {
        var result = TermsCrossCheck.Check(new ExtractedFigures(40, Gbp(10), Gbp(200), 100), "40x wagering. 100% bonus up to £200.");

        result.Status.ShouldBe(VerificationStatus.Provisional);
        result.Checks.Single(c => c.Field == OfferField.MinimumDeposit).Outcome.ShouldBe(FieldCheckOutcome.Unverifiable);
    }

    [Fact]
    public void Flags_a_figure_the_model_missed()
    {
        var result = TermsCrossCheck.Check(new ExtractedFigures(null, Gbp(10), Gbp(200), 100), Terms);

        result.Status.ShouldBe(VerificationStatus.Provisional);
        var wagering = result.Checks.Single(c => c.Field == OfferField.WageringMultiplier);
        wagering.Outcome.ShouldBe(FieldCheckOutcome.Contradicted);
        wagering.Extracted.ShouldBe("nothing");
    }

    [Fact]
    public void Treats_a_field_neither_side_has_as_absent_not_blocking()
    {
        var result = TermsCrossCheck.Check(new ExtractedFigures(40, Gbp(10), Gbp(200), null), "Min £10 deposit. Get £200. 40x wagering.");

        result.Status.ShouldBe(VerificationStatus.Confirmed);
        result.Checks.Single(c => c.Field == OfferField.MatchPercent).Outcome.ShouldBe(FieldCheckOutcome.Absent);
    }

    [Fact]
    public void Treats_a_currency_mismatch_as_a_contradiction()
    {
        var result = TermsCrossCheck.Check(new ExtractedFigures(40, Money.Create(10, "EUR").Value, Gbp(200), 100), Terms);

        result.Checks.Single(c => c.Field == OfferField.MinimumDeposit).Outcome.ShouldBe(FieldCheckOutcome.Contradicted);
    }

    private static Money Gbp(decimal amount) => Money.Create(amount, "GBP").Value;
}
