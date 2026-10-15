import { PublishedOffer, PublishedOffers, PublishedVerification } from './published-offers';

/**
 * Pure derivations for the page. Nothing here touches Angular, so each function can be
 * reasoned about (and tested) on its own.
 */

export type BadgeKind = 'checked' | 'reviewText' | 'needsReview';
export type RunStatus = 'succeeded' | 'partial' | null;

export interface SummaryCell {
  readonly label: string;
  readonly value: string;
  readonly note: string;
}

export function wagering(offer: PublishedOffer): string {
  const multiplier = offer.wageringRequirement;
  if (multiplier === null) {
    return 'Not stated';
  }

  return multiplier === 0 ? 'No wagering' : `${multiplier}x`;
}

export function host(url: string): string {
  return new URL(url).hostname.replace(/^www\./, '');
}

export function badgeKind(verification: PublishedVerification): BadgeKind {
  if (verification.status !== 'confirmed') {
    return 'needsReview';
  }

  return verification.basis === 'reviewArticle' ? 'reviewText' : 'checked';
}

export function runStatus(published: PublishedOffers): RunStatus {
  if (published.failures.length > 0) {
    return 'partial';
  }

  return published.outcome === 'succeeded' ? 'succeeded' : null;
}

/** The first 8 and last 6 characters, for example "01a13e08…f3927e". */
export function shortRunId(runId: string): string {
  return runId.length <= 14 ? runId : `${runId.slice(0, 8)}…${runId.slice(-6)}`;
}

/** "azure-openai:gpt-5.5-2026-04-24" becomes "azure-openai:gpt-5.5". */
export function withoutDateSuffix(extractedBy: string): string {
  return extractedBy.replace(/-\d{4}-\d{2}-\d{2}$/, '');
}

/** "azure-openai:gpt-5.5-2026-04-24" becomes "gpt-5.5"; several different models read "Mixed". */
export function modelFamily(casinos: readonly PublishedOffer[]): string {
  const families = new Set(
    casinos.map((c) => withoutDateSuffix(c.extractedBy.slice(c.extractedBy.indexOf(':') + 1))),
  );
  if (families.size === 0) {
    return 'Not stated';
  }

  return families.size === 1 ? [...families][0] : 'Mixed';
}

export function provenance(offer: PublishedOffer): string {
  const time = new Date(offer.extractedAt).toISOString().slice(11, 19);
  const basis =
    offer.verification.basis === 'operatorTerms'
      ? "checked against operator's terms"
      : 'checked against review text';
  return `Extracted by ${withoutDateSuffix(offer.extractedBy)} · ${time} UTC · ${basis}`;
}

export function initial(casino: string): string {
  return casino.trim().charAt(0).toUpperCase();
}

function shortName(casino: string): string {
  return casino.replace(/ Casino$/, '');
}

function namesAt(offers: readonly PublishedOffer[]): string {
  return offers.map((o) => shortName(o.casino)).join(' and ');
}

function lowestWagering(casinos: readonly PublishedOffer[]): SummaryCell {
  const stated = casinos.filter((c) => c.wageringRequirement !== null);
  if (stated.length === 0) {
    return { label: 'Lowest wagering', value: 'Not stated', note: '' };
  }

  const lowest = Math.min(...stated.map((c) => c.wageringRequirement as number));
  const at = stated.filter((c) => c.wageringRequirement === lowest);
  return { label: 'Lowest wagering', value: wagering(at[0]), note: namesAt(at) };
}

function lowestDeposit(casinos: readonly PublishedOffer[]): SummaryCell {
  const stated = casinos.filter((c) => c.details.minimumDeposit !== null);
  if (stated.length === 0) {
    return { label: 'Lowest min. deposit', value: 'Not stated', note: '' };
  }

  // Amounts in different currencies cannot be compared, so only the most common one is.
  const counts = new Map<string, number>();
  for (const c of stated) {
    const currency = c.details.minimumDeposit!.currency;
    counts.set(currency, (counts.get(currency) ?? 0) + 1);
  }
  const currency = [...counts.entries()].sort((a, b) => b[1] - a[1])[0][0];
  const inCurrency = stated.filter((c) => c.details.minimumDeposit!.currency === currency);
  const lowest = Math.min(...inCurrency.map((c) => c.details.minimumDeposit!.amount));
  const at = inCurrency.filter((c) => c.details.minimumDeposit!.amount === lowest);
  return { label: 'Lowest min. deposit', value: `${lowest} ${currency}`, note: namesAt(at) };
}

export function summary(published: PublishedOffers): readonly SummaryCell[] {
  const { casinos, failures } = published;
  const confirmed = casinos.filter((c) => c.verification.status === 'confirmed').length;
  const flagged = casinos.length - confirmed;

  return [
    {
      label: 'Casinos tracked',
      value: String(casinos.length + failures.length),
      note: failures.length
        ? `${failures.length} not updated on the last run`
        : 'All updated on the last run',
    },
    {
      label: 'Figures confirmed',
      value: `${confirmed} / ${casinos.length}`,
      note: flagged === 0 ? 'Every figure found in the source' : `${flagged} flagged for review`,
    },
    lowestWagering(casinos),
    lowestDeposit(casinos),
  ];
}
