namespace BonusScraper.Infrastructure.Crawling;

/// <summary>One allow or disallow line, with its path pattern compiled for matching.</summary>
internal sealed record RobotsRule(bool Allow, string Pattern)
{
    /// <summary>RFC 9309 section 2.2.2: the most specific match is the one with the most octets.</summary>
    public int Specificity => Pattern.Length;

    public bool Matches(string path)
    {
        var anchored = Pattern.EndsWith('$');
        var body = anchored ? Pattern[..^1] : Pattern;
        return Match(body, 0, path, 0, anchored);
    }

    private static bool Match(string pattern, int p, string path, int s, bool anchored)
    {
        while (p < pattern.Length)
        {
            if (pattern[p] == '*')
            {
                for (var skip = s; skip <= path.Length; skip++)
                {
                    if (Match(pattern, p + 1, path, skip, anchored))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (s >= path.Length || pattern[p] != path[s])
            {
                return false;
            }

            p++;
            s++;
        }

        return !anchored || s == path.Length;
    }
}
