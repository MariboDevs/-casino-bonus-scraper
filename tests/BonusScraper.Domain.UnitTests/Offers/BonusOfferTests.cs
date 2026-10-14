using BonusScraper.Domain.Monetary;
using BonusScraper.Domain.Offers;
using BonusScraper.Domain.Verification;

namespace BonusScraper.Domain.UnitTests.Offers;

public sealed class BonusOfferTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 14, 6, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Verifies_itself_against_its_own_raw_terms()
    {
        var offer = Create(wagering: 40, rawTerms: "Min £10 deposit. 100% bonus up to £200. 40x wagering.").Value;

        offer.Verification.Status.ShouldBe(VerificationStatus.Confirmed);
        offer.RawTerms.ShouldContain("40x wagering");
    }

    [Fact]
    public void Is_provisional_when_the_terms_disagree() =>
        Create(wagering: 35, rawTerms: "Min £10 deposit. 100% bonus up to £200. 40x wagering.")
            .Value.Verification.Status.ShouldBe(VerificationStatus.Provisional);

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public void Refuses_to_exist_without_its_raw_terms(string? rawTerms) =>
        Create(wagering: 40, rawTerms: rawTerms).FirstError.ShouldBe(OfferErrors.RawTermsMissing);

    private static ErrorOr.ErrorOr<BonusOffer> Create(decimal wagering, string? rawTerms)
    {
        var gbp200 = Money.Create(200, "GBP").Value;
        return BonusOffer.Create(
            CasinoName.Create("888 Casino").Value,
            WelcomeBonus.Create("100% up to 200 GBP", 100, gbp200, null).Value,
            WageringRequirement.Create(wagering).Value,
            Money.Create(10, "GBP").Value,
            SourceUrl.Create(new Uri("https://example.com/888")).Value,
            rawTerms,
            "test-model",
            Now);
    }
}
