using BonusScraper.Application.Abstractions;
using BonusScraper.Domain.Runs;
using ErrorOr;

namespace BonusScraper.Infrastructure.Publishing;

/// <summary>Publishes to every configured target; the run fails if any target fails.</summary>
internal sealed class CompositeOfferPublisher(IReadOnlyList<IOfferPublisher> publishers) : IOfferPublisher
{
    public async Task<ErrorOr<string>> PublishAsync(ScrapeRun run, CancellationToken cancellationToken)
    {
        var locations = new List<string>();
        foreach (var publisher in publishers)
        {
            var result = await publisher.PublishAsync(run, cancellationToken);
            if (result.IsError)
            {
                return result.Errors;
            }

            locations.Add(result.Value);
        }

        return string.Join(", ", locations);
    }
}
