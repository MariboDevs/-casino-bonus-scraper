using FluentValidation;

namespace BonusScraper.Application.Scraping;

public sealed class ScrapeCasinosCommandValidator : AbstractValidator<ScrapeCasinosCommand>
{
    /// <summary>A run is one politely-throttled pass; anything larger belongs in a queue, not one command.</summary>
    public const int MaxTargetsPerRun = 500;

    public ScrapeCasinosCommandValidator()
    {
        RuleFor(c => c.Targets)
            .NotEmpty().WithErrorCode("Scrape.NoTargets").WithMessage("At least one casino must be configured.")
            .Must(t => t.Count <= MaxTargetsPerRun).WithErrorCode("Scrape.TooManyTargets")
            .WithMessage($"A single run is limited to {MaxTargetsPerRun} casinos.")
            .Must(t => t.Select(x => x.Name.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() == t.Count)
            .WithErrorCode("Scrape.DuplicateTargets").WithMessage("Each casino may appear only once.");

        RuleForEach(c => c.Targets).ChildRules(target =>
        {
            target.RuleFor(t => t.Name).NotEmpty().WithErrorCode("Scrape.TargetNameMissing");
            target.RuleFor(t => t.ReviewUrl)
                .Must(u => u.IsAbsoluteUri && u.Scheme == Uri.UriSchemeHttps)
                .WithErrorCode("Scrape.TargetUrlNotHttps")
                .WithMessage("Review URLs must be absolute https addresses.");
        });
    }
}
