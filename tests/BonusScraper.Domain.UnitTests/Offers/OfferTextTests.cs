using BonusScraper.Domain.Offers;
using BonusScraper.Domain.Verification;

namespace BonusScraper.Domain.UnitTests.Offers;

public sealed class OfferTextTests
{
    [Fact]
    public void Treats_the_operator_terms_as_authoritative_when_present()
    {
        var text = OfferText.Create("Min £10 deposit.", "Article prose.").Value;

        text.Basis.ShouldBe(VerificationBasis.OperatorTerms);
        text.Authoritative.ShouldBe("Min £10 deposit.");
        text.Full.ShouldBe("Min £10 deposit.\n\nArticle prose.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void Falls_back_to_the_article(string? operatorTerms)
    {
        var text = OfferText.Create(operatorTerms, "Article prose.").Value;

        text.Basis.ShouldBe(VerificationBasis.ReviewArticle);
        text.Authoritative.ShouldBe("Article prose.");
        text.Full.ShouldBe("Article prose.");
    }

    [Fact]
    public void Requires_some_text() =>
        OfferText.Create(" ", null).FirstError.ShouldBe(OfferErrors.RawTermsMissing);
}
