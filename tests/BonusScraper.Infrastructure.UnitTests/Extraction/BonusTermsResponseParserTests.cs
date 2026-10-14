using BonusScraper.Infrastructure.Extraction;

namespace BonusScraper.Infrastructure.UnitTests.Extraction;

public sealed class BonusTermsResponseParserTests
{
    [Fact]
    public void Parses_a_schema_shaped_answer()
    {
        const string json = """
            {"welcome_bonus_summary":"100% up to 200 GBP","match_percent":100,"max_bonus_amount":200,
             "free_spins":null,"wagering_multiplier":40,"minimum_deposit_amount":10,"currency":"GBP"}
            """;

        var terms = BonusTermsResponseParser.Parse(json, "model-x").Value;

        terms.WelcomeBonusSummary.ShouldBe("100% up to 200 GBP");
        terms.MatchPercent.ShouldBe(100);
        terms.MaxBonusAmount.ShouldBe(200m);
        terms.FreeSpins.ShouldBeNull();
        terms.WageringMultiplier.ShouldBe(40m);
        terms.MinimumDepositAmount.ShouldBe(10m);
        terms.Currency.ShouldBe("GBP");
        terms.Model.ShouldBe("model-x");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Rejects_an_empty_answer(string? json) =>
        BonusTermsResponseParser.Parse(json, "m").FirstError.ShouldBe(ExtractorErrors.EmptyResponse);

    [Theory]
    [InlineData("not json")]
    [InlineData("{\"welcome_bonus_summary\":\"\"}")]
    [InlineData("{\"match_percent\":\"lots\"}")]
    public void Rejects_an_answer_that_does_not_fit_the_schema(string json) =>
        BonusTermsResponseParser.Parse(json, "m").FirstError.ShouldBe(ExtractorErrors.MalformedResponse);
}
