/**
 * The JSON contract the scraper publishes (schema version 1). It mirrors
 * PublishedOffers in BonusScraper.Infrastructure; a change there is a change here.
 */
export interface PublishedOffers {
  readonly schemaVersion: number;
  readonly generatedAt: string;
  readonly runId: string;
  readonly source: string;
  readonly outcome: 'succeeded' | 'partiallySucceeded' | 'failed';
  readonly casinos: readonly PublishedOffer[];
  readonly failures: readonly PublishedFailure[];
}

export interface PublishedOffer {
  readonly casino: string;
  readonly welcomeBonus: string;
  readonly wageringRequirement: number | null;
  readonly minimumDeposit: string | null;
  readonly sourceUrl: string;
  readonly details: PublishedOfferDetails;
  readonly verification: PublishedVerification;
  readonly extractedBy: string;
  readonly extractedAt: string;
  /**
   * True when the model's answer was given on an earlier run for the identical page text and
   * reused; extractedAt is then when the page was fetched on this run. Absent in older files.
   */
  readonly answerReused?: boolean;
}

export interface PublishedOfferDetails {
  readonly matchPercent: number | null;
  readonly maxBonus: PublishedMoney | null;
  readonly freeSpins: number | null;
  readonly minimumDeposit: PublishedMoney | null;
}

export interface PublishedMoney {
  readonly amount: number;
  readonly currency: string;
}

export interface PublishedVerification {
  readonly status: 'confirmed' | 'provisional';
  /** Which text the check read: the operator's own terms, or only the review's article. */
  readonly basis: 'operatorTerms' | 'reviewArticle';
  readonly notes: readonly string[];
}

export interface PublishedFailure {
  readonly casino: string;
  readonly sourceUrl: string;
  readonly code: string;
}

export const SUPPORTED_SCHEMA_VERSION = 1;
