using ErrorOr;

namespace BonusScraper.Domain.Offers;

/// <summary>The absolute http(s) address an offer was read from.</summary>
public sealed record SourceUrl
{
    private SourceUrl(Uri value) => Value = value;

    public Uri Value { get; }

    public static ErrorOr<SourceUrl> Create(Uri? value)
    {
        if (value is null || !value.IsAbsoluteUri)
        {
            return OfferErrors.SourceUrlNotAbsolute;
        }

        return value.Scheme is "https" or "http" ? new SourceUrl(value) : OfferErrors.SourceUrlNotHttp;
    }

    public override string ToString() => Value.AbsoluteUri;
}
