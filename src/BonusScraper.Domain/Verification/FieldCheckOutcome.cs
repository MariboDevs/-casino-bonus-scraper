namespace BonusScraper.Domain.Verification;

public enum FieldCheckOutcome
{
    /// <summary>The raw terms state the same value the language model extracted.</summary>
    Corroborated,

    /// <summary>The raw terms state a different value: the model output is not trusted.</summary>
    Contradicted,

    /// <summary>The model returned a value the raw terms never state in a recognisable form.</summary>
    Unverifiable,

    /// <summary>Neither the model nor the raw terms produced a value.</summary>
    Absent,
}
