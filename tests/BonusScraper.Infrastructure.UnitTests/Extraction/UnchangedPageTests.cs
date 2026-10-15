using BonusScraper.Application.Abstractions;
using BonusScraper.Infrastructure.Extraction;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BonusScraper.Infrastructure.UnitTests.Extraction;

public sealed class UnchangedPageTests : IDisposable
{
    private static readonly BonusTermsDocument Document = new("888 Casino", "Min £10 deposit. 40x wagering.");
    private static readonly ExtractedBonusTerms Yesterday = new("100% up to 200 GBP", 100, 200, null, 40, 10, "GBP", "live-model");
    private static readonly ExtractedBonusTerms Today = new("100% up to 300 GBP", 100, 300, null, 35, 20, "GBP", "live-model");

    private readonly string _directory = Path.Combine(Path.GetTempPath(), "bonus-unchanged-" + Guid.NewGuid().ToString("N"));
    private readonly IBonusTermsExtractor _live = Substitute.For<IBonusTermsExtractor>();

    [Fact]
    public async Task An_unchanged_page_reuses_the_recorded_answer_without_asking_the_model()
    {
        await RecordYesterday();

        var result = await Extractor().ExtractAsync(Document, CancellationToken.None);

        result.Value.ShouldBe(Yesterday with { Reused = true });
        await _live.DidNotReceive().ExtractAsync(Arg.Any<BonusTermsDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_changed_page_goes_to_the_model()
    {
        await RecordYesterday();
        var changed = Document with { Text = "Min £20 deposit. 35x wagering." };
        _live.ExtractAsync(changed, Arg.Any<CancellationToken>()).Returns(Today);

        var result = await Extractor().ExtractAsync(changed, CancellationToken.None);

        result.Value.ShouldBe(Today);
        result.Value.Reused.ShouldBeFalse();
    }

    [Fact]
    public async Task With_no_recordings_every_page_goes_to_the_model()
    {
        _live.ExtractAsync(Document, Arg.Any<CancellationToken>()).Returns(Today);

        var result = await Extractor().ExtractAsync(Document, CancellationToken.None);

        result.Value.ShouldBe(Today);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private async Task RecordYesterday()
    {
        var yesterday = Substitute.For<IBonusTermsExtractor>();
        yesterday.ExtractAsync(Document, Arg.Any<CancellationToken>()).Returns(Yesterday);
        await new RecordingBonusTermsExtractor(yesterday, _directory).ExtractAsync(Document, CancellationToken.None);
    }

    private UnchangedPageBonusTermsExtractor Extractor() =>
        new(
            new ReplayBonusTermsExtractor(Options.Create(new ExtractionOptions { RecordingsPath = _directory })),
            _live,
            NullLogger<UnchangedPageBonusTermsExtractor>.Instance);
}
