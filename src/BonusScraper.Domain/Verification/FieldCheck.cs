namespace BonusScraper.Domain.Verification;

/// <summary>The cross-check's verdict on one field, with the values it compared.</summary>
public sealed record FieldCheck(OfferField Field, FieldCheckOutcome Outcome, string? Extracted, string? FoundInTerms)
{
    public bool BlocksConfirmation => Outcome is FieldCheckOutcome.Contradicted or FieldCheckOutcome.Unverifiable;

    public string Describe() => Outcome switch
    {
        FieldCheckOutcome.Corroborated => $"{Field}: {Extracted} matches the terms.",
        FieldCheckOutcome.Contradicted => $"{Field}: extracted {Extracted} but the terms say {FoundInTerms}.",
        FieldCheckOutcome.Unverifiable => $"{Field}: extracted {Extracted} could not be found in the terms.",
        FieldCheckOutcome.Absent => $"{Field}: not stated.",
        _ => throw new ArgumentOutOfRangeException(nameof(Outcome), Outcome, "Unhandled outcome."),
    };
}
