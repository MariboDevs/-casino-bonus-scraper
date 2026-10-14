namespace BonusScraper.Infrastructure.Extraction;

/// <summary>
/// The JSON schema both providers are constrained to. Strict-mode compatible: every
/// property is required and absence is expressed as null, not as a missing key.
/// </summary>
internal static class BonusTermsSchema
{
    public const string Name = "bonus_terms";

    public const string Json = """
        {
          "type": "object",
          "additionalProperties": false,
          "required": [
            "welcome_bonus_summary",
            "match_percent",
            "max_bonus_amount",
            "free_spins",
            "wagering_multiplier",
            "minimum_deposit_amount",
            "currency"
          ],
          "properties": {
            "welcome_bonus_summary": { "type": "string" },
            "match_percent": { "type": ["integer", "null"] },
            "max_bonus_amount": { "type": ["number", "null"] },
            "free_spins": { "type": ["integer", "null"] },
            "wagering_multiplier": { "type": ["number", "null"] },
            "minimum_deposit_amount": { "type": ["number", "null"] },
            "currency": { "type": ["string", "null"] }
          }
        }
        """;
}
