using BonusScraper.Infrastructure.Crawling;

namespace BonusScraper.Infrastructure.UnitTests.Crawling;

public sealed class RobotsTxtTests
{
    private const string Bot = "BonusScraperBot";

    [Fact]
    public void Allows_the_review_pages_on_the_real_source()
    {
        var robots = RobotsTxt.Parse(Fixture.Read("casinoguide-robots.txt"));

        robots.IsAllowed(Bot, new Uri("https://www.casinoguide.co.uk/888-casino-review")).ShouldBeTrue();
        robots.IsAllowed(Bot, new Uri("https://www.casinoguide.co.uk/goto/888")).ShouldBeFalse();
        robots.IsAllowed(Bot, new Uri("https://www.casinoguide.co.uk/wp-admin/options.php")).ShouldBeFalse();
    }

    [Fact]
    public void Lets_the_longest_matching_rule_win()
    {
        var robots = RobotsTxt.Parse("""
            User-agent: *
            Disallow: /reviews/
            Allow: /reviews/public/
            """);

        robots.IsAllowed(Bot, new Uri("https://x.example/reviews/secret")).ShouldBeFalse();
        robots.IsAllowed(Bot, new Uri("https://x.example/reviews/public/888")).ShouldBeTrue();
    }

    [Fact]
    public void Prefers_allow_when_rules_are_equally_specific()
    {
        var robots = RobotsTxt.Parse("""
            User-agent: *
            Disallow: /page
            Allow: /page
            """);

        robots.IsAllowed(Bot, new Uri("https://x.example/page")).ShouldBeTrue();
    }

    [Theory]
    [InlineData("/goto/888", false)]
    [InlineData("/casinos/goto/888/", false)]
    [InlineData("/goodbye", true)]
    public void Supports_wildcards(string path, bool allowed) =>
        RobotsTxt.Parse("User-agent: *\nDisallow: */goto/*")
            .IsAllowed(Bot, new Uri("https://x.example" + path)).ShouldBe(allowed);

    [Theory]
    [InlineData("/feed.xml", false)]
    [InlineData("/feed.xml?page=2", true)]
    public void Supports_end_anchors(string path, bool allowed) =>
        RobotsTxt.Parse("User-agent: *\nDisallow: /*.xml$")
            .IsAllowed(Bot, new Uri("https://x.example" + path)).ShouldBe(allowed);

    [Fact]
    public void Uses_the_group_naming_this_crawler_instead_of_the_wildcard_group()
    {
        var robots = RobotsTxt.Parse("""
            User-agent: *
            Disallow: /

            User-agent: BonusScraperBot
            Disallow: /private/
            """);

        robots.IsAllowed(Bot, new Uri("https://x.example/888")).ShouldBeTrue();
        robots.IsAllowed("OtherBot", new Uri("https://x.example/888")).ShouldBeFalse();
    }

    [Fact]
    public void Shares_rules_between_consecutive_user_agent_lines()
    {
        var robots = RobotsTxt.Parse("""
            User-agent: SomeBot
            User-agent: BonusScraperBot
            Disallow: /blocked
            """);

        robots.IsAllowed(Bot, new Uri("https://x.example/blocked")).ShouldBeFalse();
    }

    [Fact]
    public void Reads_the_crawl_delay_for_the_matching_group()
    {
        var robots = RobotsTxt.Parse("""
            User-agent: *
            Crawl-delay: 5
            """);

        robots.CrawlDelayFor(Bot).ShouldBe(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Ignores_comments_and_empty_disallows()
    {
        var robots = RobotsTxt.Parse("""
            # Everything is allowed
            User-agent: * # all crawlers
            Disallow:
            """);

        robots.IsAllowed(Bot, new Uri("https://x.example/anything")).ShouldBeTrue();
    }

    [Fact]
    public void Always_allows_robots_txt_itself() =>
        RobotsTxt.DisallowAll.IsAllowed(Bot, new Uri("https://x.example/robots.txt")).ShouldBeTrue();

    [Fact]
    public void Treats_an_empty_file_as_allowing_everything() =>
        RobotsTxt.Parse(string.Empty).IsAllowed(Bot, new Uri("https://x.example/a")).ShouldBeTrue();
}
