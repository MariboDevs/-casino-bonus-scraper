using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace BonusScraper.Infrastructure.Crawling;

/// <summary>
/// Spaces requests to the same host by the larger of the configured minimum and the
/// host's robots.txt Crawl-delay. Requests to one host are serialised; different hosts
/// proceed independently.
/// </summary>
internal sealed class PoliteCrawlHandler(IRobotsPolicy robots, IOptions<CrawlerOptions> options, TimeProvider clock)
    : DelegatingHandler
{
    private readonly ConcurrentDictionary<string, HostSlot> _hosts = new(StringComparer.OrdinalIgnoreCase);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri ?? throw new InvalidOperationException("A crawl request must have an absolute URI.");
        var decision = await robots.EvaluateAsync(uri, cancellationToken);
        var gap = TimeSpan.FromSeconds(Math.Max(options.Value.MinimumDelaySeconds, decision.CrawlDelay?.TotalSeconds ?? 0));

        var slot = _hosts.GetOrAdd(uri.Host, _ => new HostSlot());
        await slot.Gate.WaitAsync(cancellationToken);
        try
        {
            if (slot.LastRequestAt is { } last)
            {
                var wait = last + gap - clock.GetUtcNow();
                if (wait > TimeSpan.Zero)
                {
                    await Task.Delay(wait, clock, cancellationToken);
                }
            }

            slot.LastRequestAt = clock.GetUtcNow();
            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            slot.Gate.Release();
        }
    }
}
