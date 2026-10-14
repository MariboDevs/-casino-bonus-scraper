using System.Globalization;

namespace BonusScraper.Infrastructure.Crawling;

/// <summary>
/// A parsed robots.txt, following RFC 9309: groups are selected by the most specific
/// matching product token (falling back to <c>*</c>), and within a group the longest
/// matching rule wins, with allow winning a tie. <c>Crawl-delay</c> is not in the RFC
/// but is widely published, so it is honoured as a politeness floor.
/// </summary>
internal sealed class RobotsTxt
{
    /// <summary>RFC 9309 section 2.5: parsers must handle at least 500 KiB.</summary>
    public const int MaxBytes = 500 * 1024;

    public static readonly RobotsTxt AllowAll = new([]);
    public static readonly RobotsTxt DisallowAll = new([new RobotsGroup(["*"], [new RobotsRule(false, "/")], null)]);

    private readonly IReadOnlyList<RobotsGroup> _groups;

    private RobotsTxt(IReadOnlyList<RobotsGroup> groups) => _groups = groups;

    public static RobotsTxt Parse(string content)
    {
        var groups = new List<RobotsGroup>();
        var agents = new List<string>();
        var rules = new List<RobotsRule>();
        TimeSpan? delay = null;
        var lastWasAgent = false;

        void Flush()
        {
            if (agents.Count > 0)
            {
                groups.Add(new RobotsGroup([.. agents], [.. rules], delay));
            }

            agents.Clear();
            rules.Clear();
            delay = null;
        }

        foreach (var rawLine in content.Split('\n'))
        {
            var line = StripComment(rawLine).Trim();
            var colon = line.IndexOf(':', StringComparison.Ordinal);
            if (colon <= 0)
            {
                continue;
            }

            var key = line[..colon].Trim().ToLowerInvariant();
            var value = line[(colon + 1)..].Trim();

            switch (key)
            {
                case "user-agent":
                    if (!lastWasAgent)
                    {
                        Flush();
                    }

                    agents.Add(value.ToLowerInvariant());
                    lastWasAgent = true;
                    continue;
                case "allow" or "disallow" when agents.Count > 0:
                    if (value.Length > 0)
                    {
                        rules.Add(new RobotsRule(key == "allow", NormalisePath(value)));
                    }

                    break;
                case "crawl-delay" when agents.Count > 0:
                    if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) && seconds >= 0)
                    {
                        delay = TimeSpan.FromSeconds(seconds);
                    }

                    break;
            }

            lastWasAgent = false;
        }

        Flush();
        return new RobotsTxt(groups);
    }

    public bool IsAllowed(string productToken, Uri uri)
    {
        var path = NormalisePath(uri.PathAndQuery);
        if (path == "/robots.txt")
        {
            return true;
        }

        var best = RulesFor(productToken)
            .Where(r => r.Matches(path))
            .OrderByDescending(r => r.Specificity)
            .ThenByDescending(r => r.Allow)
            .FirstOrDefault();

        return best?.Allow ?? true;
    }

    public TimeSpan? CrawlDelayFor(string productToken) =>
        GroupsFor(productToken).Select(g => g.CrawlDelay).FirstOrDefault(d => d is not null);

    private IEnumerable<RobotsRule> RulesFor(string productToken) => GroupsFor(productToken).SelectMany(g => g.Rules);

    /// <summary>All groups naming this product token, or every <c>*</c> group if none do (RFC 9309 section 2.2.1: matching groups are combined).</summary>
    private List<RobotsGroup> GroupsFor(string productToken)
    {
        var token = productToken.ToLowerInvariant();
        var specific = _groups.Where(g => g.UserAgents.Contains(token)).ToList();
        return specific.Count > 0 ? specific : _groups.Where(g => g.UserAgents.Contains("*")).ToList();
    }

    private static string StripComment(string line)
    {
        var hash = line.IndexOf('#', StringComparison.Ordinal);
        return hash >= 0 ? line[..hash] : line;
    }

    private static string NormalisePath(string path)
    {
        var decoded = Uri.UnescapeDataString(path);
        return decoded.StartsWith('/') || decoded.StartsWith('*') ? decoded : "/" + decoded;
    }
}
