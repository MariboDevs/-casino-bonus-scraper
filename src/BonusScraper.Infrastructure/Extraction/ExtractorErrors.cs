using ErrorOr;

namespace BonusScraper.Infrastructure.Extraction;

internal static class ExtractorErrors
{
    public static readonly Error EmptyResponse =
        Error.Unexpected("Extractor.EmptyResponse", "The model returned no content.");

    public static readonly Error MalformedResponse =
        Error.Unexpected("Extractor.MalformedResponse", "The model's answer did not match the bonus terms schema.");

    public static readonly Error Refused =
        Error.Failure("Extractor.Refused", "The model declined to answer.");

    public static readonly Error Truncated =
        Error.Unexpected("Extractor.Truncated", "The model's answer was cut off before it was complete.");

    public static readonly Error RecordingMissing =
        Error.NotFound("Extractor.RecordingMissing", "No recorded answer exists for this text; run once against a live model with recording on.");

    public static Error ModelUnavailable(int? status) =>
        Error.Unexpected("Extractor.ModelUnavailable", status is null
            ? "The model could not be reached."
            : $"The model endpoint answered {status}.");
}
