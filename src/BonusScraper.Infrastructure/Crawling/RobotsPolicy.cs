using System.Collections.Concurrent;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BonusScraper.Infrastructure.Crawling;

/// <summary>
/// Fetches and caches robots.txt once per origin for the lifetime of the process.
/// Availability follows RFC 9309 section 2.3.1: a 4xx means there are no rules, while
/// a 5xx or network failure means the site may not be crawled at all.
/// </summary>
internal sealed partial class RobotsPolicy(
    IHttpClientFactory clients,
    IOptions<CrawlerOptions> options,
    ILogger<RobotsPolicy> logger) : IRobotsPolicy
{
    public const string ClientName = "robots";

    private readonly ConcurrentDictionary<string, Lazy<Task<RobotsTxt>>> _cache = new(StringComparer.OrdinalIgnoreCase);

    public async Task<RobotsDecision> EvaluateAsync(Uri uri, CancellationToken cancellationToken)
    {
        var origin = uri.GetLeftPart(UriPartial.Authority);
        var robots = await _cache.GetOrAdd(origin, o => new Lazy<Task<RobotsTxt>>(() => FetchAsync(o, cancellationToken))).Value;
        var token = options.Value.ProductToken;
        return new RobotsDecision(robots.IsAllowed(token, uri), robots.CrawlDelayFor(token));
    }

    private async Task<RobotsTxt> FetchAsync(string origin, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await clients.CreateClient(ClientName)
                .GetAsync(new Uri(new Uri(origin), "/robots.txt"), HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (response.StatusCode is >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError)
            {
                LogRobotsUnavailable(origin, (int)response.StatusCode);
                return RobotsTxt.AllowAll;
            }

            if (!response.IsSuccessStatusCode)
            {
                LogRobotsUnreachable(origin, (int)response.StatusCode);
                return RobotsTxt.DisallowAll;
            }

            var content = await ReadCappedAsync(response, cancellationToken);
            return RobotsTxt.Parse(content);
        }
        catch (HttpRequestException ex)
        {
            LogRobotsFailed(ex, origin);
            return RobotsTxt.DisallowAll;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            LogRobotsFailed(ex, origin);
            return RobotsTxt.DisallowAll;
        }
    }

    private static async Task<string> ReadCappedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var buffer = new byte[RobotsTxt.MaxBytes];
        var total = 0;
        int read;
        while (total < buffer.Length && (read = await stream.ReadAsync(buffer.AsMemory(total), cancellationToken)) > 0)
        {
            total += read;
        }

        return System.Text.Encoding.UTF8.GetString(buffer, 0, total);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "No robots.txt at {Origin} ({Status}); no rules apply")]
    private partial void LogRobotsUnavailable(string origin, int status);

    [LoggerMessage(Level = LogLevel.Warning, Message = "robots.txt at {Origin} returned {Status}; treating the site as disallowed")]
    private partial void LogRobotsUnreachable(string origin, int status);

    [LoggerMessage(Level = LogLevel.Warning, Message = "robots.txt at {Origin} could not be fetched; treating the site as disallowed")]
    private partial void LogRobotsFailed(Exception exception, string origin);
}
