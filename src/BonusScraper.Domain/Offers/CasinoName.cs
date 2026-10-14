using ErrorOr;

namespace BonusScraper.Domain.Offers;

public sealed record CasinoName
{
    public const int MaxLength = 100;

    private CasinoName(string value) => Value = value;

    public string Value { get; }

    public static ErrorOr<CasinoName> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return OfferErrors.CasinoNameMissing;
        }

        var trimmed = value.Trim();
        return trimmed.Length > MaxLength ? OfferErrors.CasinoNameTooLong : new CasinoName(trimmed);
    }

    public override string ToString() => Value;
}
