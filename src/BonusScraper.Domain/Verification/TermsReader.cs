using System.Globalization;
using System.Text.RegularExpressions;
using BonusScraper.Domain.Monetary;

namespace BonusScraper.Domain.Verification;

/// <summary>
/// A deliberately conservative, deterministic reading of bonus terms. It is not an
/// alternative extractor: it only recognises the handful of phrasings that are
/// unambiguous, and its job is to corroborate or contradict the language model.
/// </summary>
public static partial class TermsReader
{
    private const string MoneyPattern =
        @"(?:(?<sym>C\$|[£€$R])\s?(?<amt>\d[\d,]*(?:\.\d+)?)(?<k>[kK])?(?![\d%])|(?<amt>\d[\d,]*(?:\.\d+)?)\s*(?<word>euros?|pounds?|dollars?|rand|EUR|GBP|USD|CAD|ZAR)\b)";

    private const decimal Thousand = 1000m;

    public static TermsFigures Read(string? terms)
    {
        if (string.IsNullOrWhiteSpace(terms))
        {
            return new TermsFigures([], [], [], []);
        }

        var text = Normalise(terms);

        return new TermsFigures(
            WageringMultipliers(text),
            MoneyAfter(MinimumDepositRegex(), text),
            MoneyAfter(MaxBonusRegex(), text),
            MatchPercents(text));
    }

    private static string Normalise(string terms) =>
        WhitespaceRegex().Replace(terms.Replace('‘', '\'').Replace('’', '\'').Replace(' ', ' '), " ");

    private static List<decimal> WageringMultipliers(string text)
    {
        var found = new List<decimal>();

        if (WageringFreeRegex().IsMatch(text))
        {
            found.Add(0m);
        }

        foreach (Match match in WageringRegex().Matches(text))
        {
            var value = match.Groups.Cast<Group>().Skip(1).First(g => g.Success).Value;
            found.Add(decimal.Parse(value, CultureInfo.InvariantCulture));
        }

        return found.Distinct().ToList();
    }

    private static List<Money> MoneyAfter(Regex regex, string text)
    {
        var found = new List<Money>();
        foreach (Match match in regex.Matches(text))
        {
            var money = ParseMoney(match);
            if (money is not null && !found.Contains(money))
            {
                found.Add(money);
            }
        }

        return found;
    }

    private static List<int> MatchPercents(string text) =>
        MatchPercentRegex().Matches(text)
            .Select(m => int.Parse(m.Groups.Cast<Group>().Skip(1).First(g => g.Success).Value, CultureInfo.InvariantCulture))
            .Distinct()
            .ToList();

    private static Money? ParseMoney(Match match)
    {
        var amountText = match.Groups["amt"].Value.Replace(",", string.Empty, StringComparison.Ordinal);
        if (!decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            return null;
        }

        if (match.Groups["k"].Success)
        {
            amount *= Thousand;
        }

        var code = match.Groups["sym"].Success
            ? CodeForSymbol(match.Groups["sym"].Value)
            : CodeForWord(match.Groups["word"].Value);

        var money = Money.Create(amount, code);
        return money.IsError ? null : money.Value;
    }

    private static string CodeForSymbol(string symbol) => symbol switch
    {
        "£" => Currency.Gbp.Code,
        "€" => Currency.Eur.Code,
        "$" => Currency.Usd.Code,
        "C$" => Currency.Cad.Code,
        "R" => Currency.Zar.Code,
        _ => symbol,
    };

    private static string CodeForWord(string word) => word.ToUpperInvariant() switch
    {
        "EURO" or "EUROS" => Currency.Eur.Code,
        "POUND" or "POUNDS" => Currency.Gbp.Code,
        "DOLLAR" or "DOLLARS" => Currency.Usd.Code,
        "RAND" => Currency.Zar.Code,
        var code => code,
    };

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"\b(?:no|zero|0x)[\s-]+wagering\b|\bwager(?:ing)?[\s-]free\b", RegexOptions.IgnoreCase)]
    private static partial Regex WageringFreeRegex();

    [GeneratedRegex(
        @"\b(\d{1,3}(?:\.\d+)?)\s?x\s+(?:the\s+)?(?:wagering|wager|playthrough|rollover)" +
        @"|\bx\s?(\d{1,3})\s+(?:wagering|wager|playthrough)" +
        @"|\bwagering(?:\s+requirements?)?(?:\s+(?:of|is))?\s*'?\s*(\d{1,3})\s?x\b" +
        @"|\b(?:wager|bet|play)\b[^.]{0,80}?\b(\d{1,3})\s+times\b" +
        @"|\bwager(?:ing)?\s+req(?:uirements?|\.)?\s*:?\s*(?:of\s+)?(\d{1,3})\s?x\b" +
        @"|\b(\d{1,3})\s?x\s+(?:[a-z]+\s+){1,2}?wager(?:ing)?\b" +
        @"|\bwagered\s+(\d{1,3})\s?x\b" +
        @"|\bwager\s+(?:the\s+)?bonus\s+(\d{1,3})\s?x\b",
        RegexOptions.IgnoreCase)]
    private static partial Regex WageringRegex();

    [GeneratedRegex(
        @"\bmin(?:imum|\.)?\s+(?:[a-z]+\s+){0,3}?deposit(?:\s+(?:is|of|amount))?\s*:?\s*" + MoneyPattern +
        @"|\bmin(?:imum|\.)?\s+(?:deposit\s+)?" + MoneyPattern + @"(?:\s+\w+)?\s+deposit" +
        @"|\bdeposit\s+" + MoneyPattern + @"\s+(?:and\s+)?get\b" +
        @"|\bmin\.?\s+dep\.?\s+" + MoneyPattern +
        @"|\bdeposit\s+(?:of\s+)?" + MoneyPattern + @"\s*(?:\+|or\s+more\b)" +
        @"|\bdeposit\s+(?:of\s+)?at\s+least\s+" + MoneyPattern,
        RegexOptions.IgnoreCase)]
    private static partial Regex MinimumDepositRegex();

    [GeneratedRegex(
        @"\bup\s+to\s+(?:a\s+)?(?:maximum\s+of\s+)?" + MoneyPattern +
        @"|\bget\s+" + MoneyPattern +
        @"|" + MoneyPattern + @"\s+(?:bonus|in\s+bonus(?:es)?)\b",
        RegexOptions.IgnoreCase)]
    private static partial Regex MaxBonusRegex();

    [GeneratedRegex(
        @"\b(\d{1,4})\s?%(?=\s*(?:match|bonus|deposit|welcome|up\s+to|first))" +
        @"|\bmatch(?:es|ed)?\s+(?:[a-z]+\s+){0,3}?(?:to|by)\s+(\d{1,4})\s?%",
        RegexOptions.IgnoreCase)]
    private static partial Regex MatchPercentRegex();
}
