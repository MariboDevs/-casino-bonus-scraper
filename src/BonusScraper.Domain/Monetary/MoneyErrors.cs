using ErrorOr;

namespace BonusScraper.Domain.Monetary;

public static class MoneyErrors
{
    public static readonly Error CurrencyMissing =
        Error.Validation("Money.CurrencyMissing", "A currency code is required.");

    public static readonly Error AmountNegative =
        Error.Validation("Money.AmountNegative", "A monetary amount cannot be negative.");

    public static Error CurrencyUnknown(string code) =>
        Error.Validation("Money.CurrencyUnknown", $"Currency '{code}' is not a recognised ISO 4217 code.");
}
