using System.Globalization;
using ErrorOr;

namespace BonusScraper.Domain.Monetary;

/// <summary>An amount in a known currency. Never negative.</summary>
public sealed record Money
{
    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public Currency Currency { get; }

    public static ErrorOr<Money> Create(decimal amount, string? currencyCode)
    {
        if (amount < 0)
        {
            return MoneyErrors.AmountNegative;
        }

        return Currency.FromCode(currencyCode).Then(currency => new Money(amount, currency));
    }

    /// <summary>Formats as the brief's example does: "20 EUR".</summary>
    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"{Amount:0.##} {Currency.Code}");
}
