using BonusScraper.Domain.Monetary;

namespace BonusScraper.Domain.UnitTests.Monetary;

public sealed class MoneyTests
{
    [Fact]
    public void Formats_like_the_brief() => Money.Create(20m, "eur").Value.ToString().ShouldBe("20 EUR");

    [Fact]
    public void Keeps_meaningful_decimals() => Money.Create(12.5m, "GBP").Value.ToString().ShouldBe("12.5 GBP");

    [Fact]
    public void Rejects_negative_amounts() =>
        Money.Create(-1m, "GBP").FirstError.ShouldBe(MoneyErrors.AmountNegative);

    [Fact]
    public void Rejects_unknown_currencies() =>
        Money.Create(1m, "XYZ").FirstError.Code.ShouldBe("Money.CurrencyUnknown");

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public void Requires_a_currency(string? code) =>
        Money.Create(1m, code).FirstError.ShouldBe(MoneyErrors.CurrencyMissing);

    [Fact]
    public void Compares_by_value_regardless_of_scale() =>
        Money.Create(200m, "GBP").Value.ShouldBe(Money.Create(200.00m, "GBP").Value);
}
