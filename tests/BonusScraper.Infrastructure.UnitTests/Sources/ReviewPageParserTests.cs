using BonusScraper.Infrastructure.Sources;

namespace BonusScraper.Infrastructure.UnitTests.Sources;

public sealed class ReviewPageParserTests
{
    [Fact]
    public void Reads_the_offer_box_headline_and_full_terms()
    {
        var page = ReviewPageParser.Parse(Fixture.Page("888"));

        page.Headline.ShouldBe("100% up to £200");
        page.Terms.ShouldNotBeNull();
        page.Terms.ShouldStartWith("18+ | For new 888casino UK/Ireland customers. Min £10 first deposit");
        page.Terms.ShouldContain("40x wagering req applies");
        page.Terms.ShouldNotContain("&pound;");
        page.Terms.ShouldNotContain("...");
    }

    [Fact]
    public void Reads_the_bonus_section_up_to_the_next_heading_of_the_same_level()
    {
        var page = ReviewPageParser.Parse(Fixture.Page("888"));

        page.Body.ShouldNotBeNull();
        page.Body.ShouldContain("888 Casino bonus");
        page.Body.ShouldNotContain("Mobile Compatibility");
    }

    [Fact]
    public void Falls_back_to_the_article_when_the_offer_box_is_empty()
    {
        var page = ReviewPageParser.Parse(Fixture.Page("casumo"));

        page.Headline.ShouldBeNull();
        page.Terms.ShouldBeNull();
        page.Body.ShouldNotBeNull();
        page.Body.ShouldContain("Wagering 30x");
        page.Body.ShouldContain("1. Casumo Welcome Bonus");
    }

    [Fact]
    public void Reads_a_deposit_and_get_headline()
    {
        var page = ReviewPageParser.Parse(Fixture.Page("grosvenor"));

        page.Headline.ShouldBe("Deposit £20 Get £30");
        page.Terms.ShouldNotBeNull();
        page.Terms.ShouldContain("(x30 wagering)");
    }

    [Fact]
    public void Returns_an_empty_page_for_markup_without_any_offer() =>
        ReviewPageParser.Parse("<html><body><p>Nothing here</p></body></html>").IsEmpty.ShouldBeTrue();

    [Fact]
    public void Caps_the_bonus_section_length()
    {
        var html = "<h2>Welcome Bonus</h2>" + string.Concat(Enumerable.Repeat("<p>" + new string('a', 500) + "</p>", 20));

        ReviewPageParser.Parse(html).Body!.Length.ShouldBeLessThanOrEqualTo(ReviewPageParser.MaxBodyCharacters);
    }
}
