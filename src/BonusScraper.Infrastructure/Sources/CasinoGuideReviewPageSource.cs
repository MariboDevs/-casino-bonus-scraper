using System.Net;
using System.Text.RegularExpressions;
using BonusScraper.Application.Abstractions;
using BonusScraper.Infrastructure.Crawling;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace BonusScraper.Infrastructure.Sources;

/// <summary>
/// Reads review pages from casinoguide.co.uk. robots.txt is checked before every request,
/// and a refusal, rate limit or challenge page ends the attempt for that page rather
/// than being worked around.
/// </summary>
internal sealed partial class CasinoGuideReviewPageSource(
    IHttpClientFactory clients,
    IRobotsPolicy robots,
    IOptions<CrawlerOptions> options,
    TimeProvider clock) : IReviewPageSource
{
    public const string ClientName = "pages";

    public async Task<ErrorOr<ReviewPage>> ReadAsync(Uri source, CancellationToken cancellationToken)
    {
        var decision = await robots.EvaluateAsync(source, cancellationToken);
        if (!decision.Allowed)
        {
            return SourceErrors.RobotsDisallowed;
        }

        if (decision.CrawlDelay is { } delay && delay.TotalSeconds > options.Value.MaximumDelaySeconds)
        {
            return SourceErrors.CrawlDelayTooLong(delay);
        }

        string html;
        try
        {
            using var response = await clients.CreateClient(ClientName).GetAsync(source, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return response.StatusCode switch
                {
                    HttpStatusCode.NotFound or HttpStatusCode.Gone => SourceErrors.PageNotFound,
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests
                        or HttpStatusCode.UnavailableForLegalReasons => SourceErrors.Blocked((int)response.StatusCode),
                    _ => SourceErrors.Failed((int)response.StatusCode),
                };
            }

            html = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (HttpRequestException)
        {
            return SourceErrors.Unreachable;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return SourceErrors.Unreachable;
        }

        var page = ReviewPageParser.Parse(html);

        // Challenge markers can appear on ordinary pages (a comment form's reCAPTCHA), so a
        // page only counts as a challenge when it also carries none of the review's content.
        if (page.IsEmpty && ChallengeRegex().IsMatch(html))
        {
            return SourceErrors.ChallengePage;
        }

        return new ReviewPage(source, page.Headline, page.Terms, page.Body, clock.GetUtcNow());
    }

    [GeneratedRegex(@"id=""challenge-form""|cf-turnstile|g-recaptcha|h-captcha|<title>\s*(Just a moment|Attention Required)", RegexOptions.IgnoreCase)]
    private static partial Regex ChallengeRegex();
}
