using BonusScraper.Application.Scraping;

namespace BonusScraper.Application.UnitTests.Scraping;

public sealed class ScrapeCasinosCommandValidatorTests
{
    private readonly ScrapeCasinosCommandValidator _validator = new();

    [Fact]
    public void Accepts_distinct_https_targets() =>
        _validator.Validate(Command(("888", "https://a.example/888"), ("Casumo", "https://a.example/casumo"))).IsValid.ShouldBeTrue();

    [Fact]
    public void Requires_at_least_one_target() =>
        Codes(Command()).ShouldContain("Scrape.NoTargets");

    [Fact]
    public void Rejects_duplicate_casinos_regardless_of_case() =>
        Codes(Command(("888", "https://a.example/1"), (" 888 ".ToUpperInvariant(), "https://a.example/2"))).ShouldContain("Scrape.DuplicateTargets");

    [Fact]
    public void Rejects_plain_http_targets() =>
        Codes(Command(("888", "http://a.example/888"))).ShouldContain("Scrape.TargetUrlNotHttps");

    [Fact]
    public void Rejects_unnamed_targets() =>
        Codes(Command(("", "https://a.example/888"))).ShouldContain("Scrape.TargetNameMissing");

    [Fact]
    public void Caps_the_size_of_one_run()
    {
        var targets = Enumerable.Range(0, ScrapeCasinosCommandValidator.MaxTargetsPerRun + 1)
            .Select(i => ($"c{i}", $"https://a.example/{i}"))
            .ToArray();

        Codes(Command(targets)).ShouldContain("Scrape.TooManyTargets");
    }

    private IEnumerable<string> Codes(ScrapeCasinosCommand command) =>
        _validator.Validate(command).Errors.Select(e => e.ErrorCode);

    private static ScrapeCasinosCommand Command(params (string Name, string Url)[] targets) =>
        new(targets.Select(t => new CasinoTarget(t.Name, new Uri(t.Url))).ToList());
}
