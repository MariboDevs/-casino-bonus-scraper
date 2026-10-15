namespace BonusScraper.Domain.Verification;

public sealed record OfferVerification(VerificationStatus Status, VerificationBasis Basis, IReadOnlyList<FieldCheck> Checks)
{
    public static OfferVerification From(VerificationBasis basis, IReadOnlyList<FieldCheck> checks) =>
        new(checks.Any(c => c.BlocksConfirmation) ? VerificationStatus.Provisional : VerificationStatus.Confirmed, basis, checks);

    public IEnumerable<string> Notes => Checks.Where(c => c.BlocksConfirmation).Select(c => c.Describe());
}
