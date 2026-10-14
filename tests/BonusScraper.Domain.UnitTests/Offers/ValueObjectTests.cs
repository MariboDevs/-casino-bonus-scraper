using BonusScraper.Domain.Monetary;
using BonusScraper.Domain.Offers;

namespace BonusScraper.Domain.UnitTests.Offers;

public sealed class ValueObjectTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(35, false)]
    public void Accepts_plausible_wagering(decimal multiplier, bool wageringFree)
    {
        var wagering = WageringRequirement.Create(multiplier).Value;

        wagering.Multiplier.ShouldBe(multiplier);
        wagering.IsWageringFree.ShouldBe(wageringFree);
    }

    [Fact]
    public void Rejects_negative_wagering() =>
        WageringRequirement.Create(-1).FirstError.ShouldBe(OfferErrors.WageringNegative);

    [Fact]
    public void Rejects_a_multiplier_that_is_really_a_percentage_or_amount() =>
        WageringRequirement.Create(500).FirstError.Code.ShouldBe("Offer.WageringImplausible");

    [Fact]
    public void Trims_casino_names() => CasinoName.Create("  888 Casino ").Value.Value.ShouldBe("888 Casino");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Requires_a_casino_name(string? name) =>
        CasinoName.Create(name).FirstError.ShouldBe(OfferErrors.CasinoNameMissing);

    [Fact]
    public void Rejects_overlong_casino_names() =>
        CasinoName.Create(new string('a', CasinoName.MaxLength + 1)).FirstError.ShouldBe(OfferErrors.CasinoNameTooLong);

    [Fact]
    public void Requires_an_absolute_source() =>
        SourceUrl.Create(new Uri("/reviews/888", UriKind.Relative)).FirstError.ShouldBe(OfferErrors.SourceUrlNotAbsolute);

    [Fact]
    public void Requires_an_http_source() =>
        SourceUrl.Create(new Uri("ftp://example.com/x")).FirstError.ShouldBe(OfferErrors.SourceUrlNotHttp);

    [Fact]
    public void Builds_a_welcome_bonus()
    {
        var bonus = WelcomeBonus.Create(" 100% up to 200 GBP ", 100, Money.Create(200, "GBP").Value, 50).Value;

        bonus.Summary.ShouldBe("100% up to 200 GBP");
        bonus.MatchPercent.ShouldBe(100);
        bonus.FreeSpins.ShouldBe(50);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public void Rejects_out_of_range_match_percentages(int percent) =>
        WelcomeBonus.Create("x", percent, null, null).FirstError.Code.ShouldBe("Offer.MatchPercentOutOfRange");

    [Fact]
    public void Rejects_negative_free_spins() =>
        WelcomeBonus.Create("x", null, null, -1).FirstError.ShouldBe(OfferErrors.FreeSpinsNegative);

    [Fact]
    public void Requires_a_summary() =>
        WelcomeBonus.Create(" ", null, null, null).FirstError.ShouldBe(OfferErrors.WelcomeBonusSummaryMissing);

    [Fact]
    public void Rejects_overlong_summaries() =>
        WelcomeBonus.Create(new string('a', WelcomeBonus.MaxSummaryLength + 1), null, null, null)
            .FirstError.ShouldBe(OfferErrors.WelcomeBonusSummaryTooLong);
}
