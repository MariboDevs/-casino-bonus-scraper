namespace BonusScraper.Domain.Verification;

/// <summary>The cross-check's verdict on one field, with the values it compared.</summary>
public sealed record FieldCheck(OfferField Field, FieldCheckOutcome Outcome, string? Extracted, string? FoundInTerms)
{
    public bool BlocksConfirmation => Outcome is FieldCheckOutcome.Contradicted or FieldCheckOutcome.Unverifiable;

    /// <summary>A sentence a reader of the page can act on; published verbatim as a verification note.</summary>
    public string Describe() => Outcome switch
    {
        FieldCheckOutcome.Corroborated => $"{Label}: {Extracted} matches the terms.",
        FieldCheckOutcome.Contradicted when Extracted == "nothing" =>
            $"{Label}: the terms say {FoundInTerms}, but the model did not report one.",
        FieldCheckOutcome.Contradicted => $"{Label}: the model read {Extracted}, but the terms say {FoundInTerms}.",
        FieldCheckOutcome.Unverifiable => $"{Label}: the model read {Extracted}, which the terms do not state.",
        FieldCheckOutcome.Absent => $"{Label}: not stated.",
        _ => throw new ArgumentOutOfRangeException(nameof(Outcome), Outcome, "Unhandled outcome."),
    };

    private string Label => Field switch
    {
        OfferField.WageringMultiplier => "Wagering",
        OfferField.MinimumDeposit => "Minimum deposit",
        OfferField.MaxBonus => "Maximum bonus",
        OfferField.MatchPercent => "Match percentage",
        _ => throw new ArgumentOutOfRangeException(nameof(Field), Field, "Unhandled field."),
    };
}
