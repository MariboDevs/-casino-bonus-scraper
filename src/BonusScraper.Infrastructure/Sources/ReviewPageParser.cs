using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace BonusScraper.Infrastructure.Sources;

/// <summary>
/// Reads the three places a casinoguide.co.uk review states its offer: the headline in
/// the offer box, the operator's significant terms beneath it, and the "Bonus" section
/// of the article. It extracts text only; deciding what the text means is the model's job.
/// </summary>
internal static partial class ReviewPageParser
{
    /// <summary>Enough for any welcome-bonus section; caps what a malformed page can send to the model.</summary>
    public const int MaxBodyCharacters = 4000;

    private static readonly HtmlParser Parser = new();

    public static ParsedReviewPage Parse(string html)
    {
        using var document = Parser.ParseDocument(html);

        return new ParsedReviewPage(
            Clean(document.QuerySelector(".single-review__bonus")?.TextContent),
            Terms(document),
            BonusSection(document));
    }

    /// <summary>
    /// The terms box carries the full text in an attribute that the site double-encodes
    /// (<c>&amp;amp;pound;</c>), so it is decoded once more after the HTML parser's pass.
    /// The visible text is truncated with an ellipsis and is only a fallback.
    /// </summary>
    private static string? Terms(IDocument document)
    {
        var element = document.QuerySelector(".cas-terms-text");
        var full = element?.GetAttribute("data-full-text");
        return Clean(string.IsNullOrWhiteSpace(full) ? element?.TextContent : WebUtility.HtmlDecode(full));
    }

    private static string? BonusSection(IDocument document)
    {
        var heading = document.QuerySelectorAll("h2, h3")
            .FirstOrDefault(h => BonusHeadingRegex().IsMatch(h.TextContent));
        if (heading is null)
        {
            return null;
        }

        var level = HeadingLevel(heading);
        var text = new StringBuilder();
        for (var node = heading.NextElementSibling; node is not null; node = node.NextElementSibling)
        {
            if (HeadingLevel(node) is { } nodeLevel && nodeLevel <= level)
            {
                break;
            }

            var part = Clean(node.TextContent);
            if (part is not null)
            {
                text.Append(part).Append('\n');
            }

            if (text.Length >= MaxBodyCharacters)
            {
                break;
            }
        }

        var body = text.ToString();
        return Clean(body.Length > MaxBodyCharacters ? body[..MaxBodyCharacters] : body);
    }

    private static int? HeadingLevel(IElement element) =>
        element.LocalName is ['h', var digit] && char.IsAsciiDigit(digit) ? digit - '0' : null;

    private static string? Clean(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var lines = text.Replace(' ', ' ')
            .Split('\n')
            .Select(l => SpacesRegex().Replace(l, " ").Trim())
            .Where(l => l.Length > 0);
        return string.Join('\n', lines);
    }

    [GeneratedRegex(@"\b(welcome\s+bonus|bonus\s+offer|welcome\s+offer|casino\s+bonus)\b", RegexOptions.IgnoreCase)]
    private static partial Regex BonusHeadingRegex();

    [GeneratedRegex(@"[ \t\r\f\v]+")]
    private static partial Regex SpacesRegex();
}
