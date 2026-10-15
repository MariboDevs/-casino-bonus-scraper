using BonusScraper.Application.Abstractions;
using BonusScraper.Infrastructure.Extraction;
using Microsoft.Extensions.Options;

namespace BonusScraper.Infrastructure.UnitTests.Extraction;

public sealed class RecordReplayTests : IDisposable
{
    private static readonly BonusTermsDocument Document = new("888 Casino", "Min £10 deposit. 40x wagering.");
    private static readonly ExtractedBonusTerms Answer = new("100% up to 200 GBP", 100, 200, null, 40, 10, "GBP", "live-model");

    private readonly string _directory = Path.Combine(Path.GetTempPath(), "bonus-recordings-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task Replays_exactly_what_a_live_model_answered_marked_as_reused()
    {
        var live = Substitute.For<IBonusTermsExtractor>();
        live.ExtractAsync(Document, Arg.Any<CancellationToken>()).Returns(Answer);

        await new RecordingBonusTermsExtractor(live, _directory).ExtractAsync(Document, CancellationToken.None);
        var replayed = await Replay().ExtractAsync(Document, CancellationToken.None);

        replayed.Value.ShouldBe(Answer with { Reused = true });
    }

    [Fact]
    public async Task Misses_when_the_page_text_has_changed()
    {
        var live = Substitute.For<IBonusTermsExtractor>();
        live.ExtractAsync(Document, Arg.Any<CancellationToken>()).Returns(Answer);
        await new RecordingBonusTermsExtractor(live, _directory).ExtractAsync(Document, CancellationToken.None);

        var replayed = await Replay().ExtractAsync(Document with { Text = "Min £20 deposit." }, CancellationToken.None);

        replayed.FirstError.ShouldBe(ExtractorErrors.RecordingMissing);
    }

    [Fact]
    public async Task Does_not_record_a_failed_answer()
    {
        var live = Substitute.For<IBonusTermsExtractor>();
        live.ExtractAsync(Document, Arg.Any<CancellationToken>()).Returns(ExtractorErrors.Refused);

        var result = await new RecordingBonusTermsExtractor(live, _directory).ExtractAsync(Document, CancellationToken.None);

        result.FirstError.ShouldBe(ExtractorErrors.Refused);
        Directory.Exists(_directory).ShouldBeFalse();
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private ReplayBonusTermsExtractor Replay() =>
        new(Options.Create(new ExtractionOptions { Provider = ExtractionProvider.Replay, RecordingsPath = _directory }));
}
