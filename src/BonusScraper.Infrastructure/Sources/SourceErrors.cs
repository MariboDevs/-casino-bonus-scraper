using ErrorOr;

namespace BonusScraper.Infrastructure.Sources;

internal static class SourceErrors
{
    public static readonly Error RobotsDisallowed =
        Error.Forbidden("Source.RobotsDisallowed", "robots.txt does not allow this page to be crawled.");

    public static readonly Error ChallengePage =
        Error.Forbidden("Source.ChallengePage", "The site answered with a bot challenge or CAPTCHA. It is not bypassed; switch source.");

    public static readonly Error PageNotFound =
        Error.NotFound("Source.PageNotFound", "The review page no longer exists.");

    public static readonly Error Unreachable =
        Error.Unexpected("Source.Unreachable", "The site could not be reached.");

    public static Error CrawlDelayTooLong(TimeSpan delay) =>
        Error.Forbidden("Source.CrawlDelayTooLong", $"robots.txt asks for {delay.TotalSeconds:0}s between requests, more than this crawler is configured to wait.");

    public static Error Blocked(int status) =>
        Error.Forbidden("Source.Blocked", $"The site refused the request ({status}). It is not retried around; switch source.");

    public static Error Failed(int status) =>
        Error.Unexpected("Source.Failed", $"The site answered {status}.");
}
