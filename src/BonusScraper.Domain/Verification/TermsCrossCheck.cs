using System.Globalization;
using BonusScraper.Domain.Offers;

namespace BonusScraper.Domain.Verification;

/// <summary>
/// Compares what the language model extracted against what <see cref="TermsReader"/>
/// can read deterministically from the authoritative text. The rule is the
/// provisional-until-confirmed rule: a model value only counts as confirmed when that
/// text independently states it.
/// </summary>
public static class TermsCrossCheck
{
    public static OfferVerification Check(ExtractedFigures extracted, OfferText text)
    {
        var figures = TermsReader.Read(text.Authoritative);

        FieldCheck[] checks =
        [
            Compare(OfferField.WageringMultiplier, extracted.WageringMultiplier.HasValue, extracted.WageringMultiplier.GetValueOrDefault(), figures.WageringMultipliers, Format),
            Compare(OfferField.MinimumDeposit, extracted.MinimumDeposit is not null, extracted.MinimumDeposit!, figures.MinimumDeposits, m => m.ToString()),
            Compare(OfferField.MaxBonus, extracted.MaxBonus is not null, extracted.MaxBonus!, figures.MaxBonuses, m => m.ToString()),
            Compare(OfferField.MatchPercent, extracted.MatchPercent.HasValue, extracted.MatchPercent.GetValueOrDefault(), figures.MatchPercents, p => $"{p}%"),
        ];

        return OfferVerification.From(text.Basis, checks);
    }

    private static FieldCheck Compare<T>(OfferField field, bool hasExtracted, T extracted, IReadOnlyList<T> found, Func<T, string> format)
        where T : notnull
    {
        var foundText = found.Count == 0 ? null : string.Join(" / ", found.Select(format));

        if (!hasExtracted)
        {
            return found.Count == 0
                ? new FieldCheck(field, FieldCheckOutcome.Absent, null, null)
                : new FieldCheck(field, FieldCheckOutcome.Contradicted, "nothing", foundText);
        }

        var extractedText = format(extracted);

        if (found.Count == 0)
        {
            return new FieldCheck(field, FieldCheckOutcome.Unverifiable, extractedText, null);
        }

        return found.Contains(extracted)
            ? new FieldCheck(field, FieldCheckOutcome.Corroborated, extractedText, foundText)
            : new FieldCheck(field, FieldCheckOutcome.Contradicted, extractedText, foundText);
    }

    private static string Format(decimal multiplier) =>
        string.Create(CultureInfo.InvariantCulture, $"{multiplier:0.##}x");
}
