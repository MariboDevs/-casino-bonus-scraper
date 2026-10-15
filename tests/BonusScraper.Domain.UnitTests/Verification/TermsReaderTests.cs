using BonusScraper.Domain.Monetary;
using BonusScraper.Domain.Verification;

namespace BonusScraper.Domain.UnitTests.Verification;

public sealed class TermsReaderTests
{
    private const string EightEightEight =
        "18+ | For new 888casino UK/Ireland customers. Min £10 first deposit (NETELLER/Skrill excluded). 100% bonus equal to the deposit, up to £200. Must claim bonus via email/pop-up/My Account within 48 hours. 40x wagering req applies and must be met within 90 days on selected games.";

    private const string Grosvenor =
        "Deposit £20 Get £30. 18+ New customers only. Min deposit £20, PayPal & Paysafe excluded. £30 bonus (x30 wagering) on selected games, Max wins £2K.";

    private const string Casumo =
        "Once you make a deposit, the casino matches it to 100%. This means you could earn bonus cash of up to 300 euros. Remember the minimum you can deposit is 100 euros. Lastly, the welcome offer includes the ‘Wagering 30x’.";

    [Fact]
    public void Reads_every_figure_from_the_888_terms()
    {
        var figures = TermsReader.Read(EightEightEight);

        figures.WageringMultipliers.ShouldBe([40m]);
        figures.MinimumDeposits.ShouldBe([Gbp(10)]);
        figures.MaxBonuses.ShouldBe([Gbp(200)]);
        figures.MatchPercents.ShouldBe([100]);
    }

    [Fact]
    public void Reads_the_prefix_x_form_and_the_deposit_get_headline()
    {
        var figures = TermsReader.Read(Grosvenor);

        figures.WageringMultipliers.ShouldBe([30m]);
        figures.MinimumDeposits.ShouldBe([Gbp(20)]);
        figures.MaxBonuses.ShouldContain(Gbp(30));
        figures.MatchPercents.ShouldBeEmpty();
    }

    [Fact]
    public void Reads_currency_words_and_curly_quoted_multipliers()
    {
        var figures = TermsReader.Read(Casumo);

        figures.WageringMultipliers.ShouldBe([30m]);
        figures.MinimumDeposits.ShouldBe([Money.Create(100, "EUR").Value]);
        figures.MaxBonuses.ShouldBe([Money.Create(300, "EUR").Value]);
        figures.MatchPercents.ShouldBe([100]);
    }

    [Theory]
    [InlineData("No wagering on free spin winnings.")]
    [InlineData("All winnings are wager-free.")]
    public void Recognises_wagering_free_offers_as_zero(string terms) =>
        TermsReader.Read(terms).WageringMultipliers.ShouldBe([0m]);

    [Fact]
    public void Expands_thousand_suffixes() =>
        TermsReader.Read("Get £2K on sign up").MaxBonuses.ShouldBe([Gbp(2000)]);

    [Fact]
    public void Does_not_read_a_return_to_player_percentage_as_a_match() =>
        TermsReader.Read("High RTP - 97.2% across slots").MatchPercents.ShouldBeEmpty();

    // Phrasings taken verbatim from operator terms on casinoguide.co.uk, 2026-10-15.
    [Theory]
    [InlineData("Wagering Requirement: 30x. Promotion available for 72 hours.", 30)]
    [InlineData("GambleAware.org New players only. Min. dep. £10. Wager req. 10x. 18+", 10)]
    [InlineData("The match-up bonus that requires a deposit must be wagered 40x.", 40)]
    [InlineData("100% Match Bonus up to £100 on 1st deposit of £20+. 50x bonus wagering applies.", 50)]
    [InlineData("Deposit and get 100% bonus up to £50; Wager bonus 10x; Withdrawals void bonus", 10)]
    public void Reads_wagering_in_other_operator_phrasings(string terms, int multiplier) =>
        TermsReader.Read(terms).WageringMultipliers.ShouldBe([multiplier]);

    [Theory]
    [InlineData("UK First Depositing Players Only. First Deposit of £10 or more to qualify.", 10)]
    [InlineData("GambleAware.org New players only. Min. dep. £10. Wager req. 10x.", 10)]
    [InlineData("100% Match Bonus up to £100 on 1st deposit of £20+.", 20)]
    [InlineData("18+. New depositing players only. Min. deposit: £20. Max. bonus: £100.", 20)]
    [InlineData("To trigger the bonus, you have to deposit at least £15.", 15)]
    public void Reads_minimum_deposits_in_other_operator_phrasings(string terms, int pounds) =>
        TermsReader.Read(terms).MinimumDeposits.ShouldBe([Gbp(pounds)]);

    [Fact]
    public void Does_not_read_a_plain_deposit_amount_as_a_minimum() =>
        TermsReader.Read("Deposit used 1st; a deposit of £50 doubles to £100.").MinimumDeposits.ShouldBeEmpty();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Returns_nothing_for_blank_terms(string? terms)
    {
        var figures = TermsReader.Read(terms);

        figures.WageringMultipliers.ShouldBeEmpty();
        figures.MinimumDeposits.ShouldBeEmpty();
        figures.MaxBonuses.ShouldBeEmpty();
        figures.MatchPercents.ShouldBeEmpty();
    }

    private static Money Gbp(decimal amount) => Money.Create(amount, "GBP").Value;
}
