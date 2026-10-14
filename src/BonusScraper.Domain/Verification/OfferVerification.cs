namespace BonusScraper.Domain.Verification;

public sealed record OfferVerification(VerificationStatus Status, IReadOnlyList<FieldCheck> Checks)
{
    public static OfferVerification From(IReadOnlyList<FieldCheck> checks) =>
        new(checks.Any(c => c.BlocksConfirmation) ? VerificationStatus.Provisional : VerificationStatus.Confirmed, checks);

    public IEnumerable<string> Notes => Checks.Where(c => c.BlocksConfirmation).Select(c => c.Describe());
}
