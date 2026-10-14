namespace BonusScraper.Domain.Verification;

/// <summary>
/// Language-model output is provisional until a deterministic reading of the raw terms
/// agrees with it. Provisional offers are still published, but flagged, so a reader
/// never mistakes an unchecked number for a checked one.
/// </summary>
public enum VerificationStatus
{
    Confirmed,
    Provisional,
}
