namespace BonusScraper.Infrastructure.UnitTests;

internal static class Fixture
{
    public static string Read(string relativePath) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", relativePath));

    public static string Page(string casinoSlug) => Read(Path.Combine("pages", $"{casinoSlug}-casino-review.html"));
}
