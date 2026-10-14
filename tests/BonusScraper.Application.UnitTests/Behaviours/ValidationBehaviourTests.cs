using BonusScraper.Application.Behaviours;
using BonusScraper.Application.Scraping;
using ErrorOr;
using Mediator;

namespace BonusScraper.Application.UnitTests.Behaviours;

public sealed class ValidationBehaviourTests
{
    [Fact]
    public async Task Returns_validation_errors_without_calling_the_handler()
    {
        var called = false;
        var behaviour = new ValidationBehaviour<ScrapeCasinosCommand, ErrorOr<ScrapeRunSummary>>([new ScrapeCasinosCommandValidator()]);

        var result = await behaviour.Handle(new ScrapeCasinosCommand([]), Next(() => called = true), CancellationToken.None);

        called.ShouldBeFalse();
        result.FirstError.Type.ShouldBe(ErrorType.Validation);
        result.FirstError.Code.ShouldBe("Scrape.NoTargets");
    }

    [Fact]
    public async Task Calls_the_handler_when_the_message_is_valid()
    {
        var called = false;
        var behaviour = new ValidationBehaviour<ScrapeCasinosCommand, ErrorOr<ScrapeRunSummary>>([new ScrapeCasinosCommandValidator()]);
        var command = new ScrapeCasinosCommand([new CasinoTarget("888", new Uri("https://a.example/888"))]);

        await behaviour.Handle(command, Next(() => called = true), CancellationToken.None);

        called.ShouldBeTrue();
    }

    [Fact]
    public async Task Passes_through_when_no_validator_is_registered()
    {
        var called = false;
        var behaviour = new ValidationBehaviour<ScrapeCasinosCommand, ErrorOr<ScrapeRunSummary>>([]);

        await behaviour.Handle(new ScrapeCasinosCommand([]), Next(() => called = true), CancellationToken.None);

        called.ShouldBeTrue();
    }

    private static MessageHandlerDelegate<ScrapeCasinosCommand, ErrorOr<ScrapeRunSummary>> Next(Action onCalled) =>
        (_, _) =>
        {
            onCalled();
            return ValueTask.FromResult<ErrorOr<ScrapeRunSummary>>(Error.Unexpected("Test.NotUsed", "Unused."));
        };
}
