using ErrorOr;

namespace BonusScraper.Application.Abstractions;

/// <summary>Fetches and reads one review page. Implementations must honour robots.txt.</summary>
public interface IReviewPageSource
{
    Task<ErrorOr<ReviewPage>> ReadAsync(Uri source, CancellationToken cancellationToken);
}
