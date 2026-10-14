using System.Security.Cryptography;
using System.Text;
using BonusScraper.Application.Abstractions;

namespace BonusScraper.Infrastructure.Extraction;

/// <summary>A recorded model answer, keyed by everything that could change the answer.</summary>
internal sealed record Recording(string Key, string Casino, string PromptVersion, ExtractedBonusTerms Terms)
{
    /// <summary>
    /// A hash of the prompt version and the exact input, so a changed page or a changed
    /// prompt misses the recording rather than replaying an answer to a different question.
    /// </summary>
    public static string KeyFor(BonusTermsDocument document)
    {
        var input = $"{BonusTermsPrompt.Version}\n{document.Casino}\n{document.Text}";
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(input)))[..16];
    }

    public static string FileName(BonusTermsDocument document) =>
        $"{Slug(document.Casino)}-{KeyFor(document)}.json";

    private static string Slug(string casino) =>
        new(casino.ToLowerInvariant().Select(c => char.IsAsciiLetterOrDigit(c) ? c : '-').ToArray());
}
