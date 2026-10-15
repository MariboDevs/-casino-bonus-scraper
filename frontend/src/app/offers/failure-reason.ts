/**
 * Plain-language reasons for the error codes the scraper publishes. The code itself stays
 * available to maintainers as a tooltip; readers of the page see the sentence.
 */
const REASONS: Readonly<Record<string, string>> = {
  'Source.Blocked': 'The review site refused the request.',
  'Source.ChallengePage':
    'The review site asked for a bot check, which the scraper does not bypass.',
  'Source.RobotsDisallowed': "The review site's robots.txt does not allow this page.",
  'Source.CrawlDelayTooLong':
    'The review site asks for longer gaps between requests than the scraper waits.',
  'Source.PageNotFound': 'The review page no longer exists.',
  'Source.Unreachable': 'The review site could not be reached.',
  'Source.Failed': 'The review site returned an error.',
  'Scrape.NoBonusText': 'The page had no bonus text to read.',
  'Extractor.RecordingMissing': 'No recorded model answer exists for the current page text.',
  'Extractor.ModelUnavailable': 'The language model could not be reached.',
  'Extractor.Refused': 'The language model declined to answer.',
  'Extractor.Truncated': "The language model's answer was cut off.",
  'Extractor.EmptyResponse': 'The language model returned no answer.',
  'Extractor.MalformedResponse': "The language model's answer did not fit the expected fields.",
};

export function failureReason(code: string): string {
  return REASONS[code] ?? 'The offer could not be read on the last run.';
}
