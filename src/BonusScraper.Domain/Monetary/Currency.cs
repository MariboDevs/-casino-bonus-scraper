using ErrorOr;

namespace BonusScraper.Domain.Monetary;

/// <summary>
/// An ISO 4217 currency the scraper recognises. The set is closed on purpose: an
/// unknown symbol is an extraction problem to surface, not a currency to invent.
/// </summary>
public sealed record Currency
{
    public static readonly Currency Gbp = new("GBP", "£");
    public static readonly Currency Eur = new("EUR", "€");
    public static readonly Currency Usd = new("USD", "$");
    public static readonly Currency Cad = new("CAD", "C$");
    public static readonly Currency Zar = new("ZAR", "R");

    private static readonly Currency[] Known = [Gbp, Eur, Usd, Cad, Zar];

    private Currency(string code, string symbol)
    {
        Code = code;
        Symbol = symbol;
    }

    public string Code { get; }

    public string Symbol { get; }

    public static ErrorOr<Currency> FromCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return MoneyErrors.CurrencyMissing;
        }

        var match = Array.Find(Known, c => string.Equals(c.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));
        return match is null ? MoneyErrors.CurrencyUnknown(code) : match;
    }

    public override string ToString() => Code;
}
