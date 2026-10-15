import { ChangeDetectionStrategy, Component, input, linkedSignal } from '@angular/core';
import { host, initial, provenance, wagering } from '../offers/offer-view';
import { PublishedMoney, PublishedOffer } from '../offers/published-offers';
import { VerificationBadge } from '../verification-badge/verification-badge';

interface FigureTile {
  readonly label: string;
  readonly value: string;
  readonly muted: boolean;
}

/**
 * The offers: an expandable table at 1040 px and wider, a card grid below that. Both are
 * rendered and a media query shows one, so the layout never depends on a resize listener.
 */
@Component({
  selector: 'app-offer-table',
  imports: [VerificationBadge],
  templateUrl: './offer-table.html',
  styleUrl: './offer-table.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OfferTable {
  readonly offers = input.required<readonly PublishedOffer[]>();

  /** Open rows, keyed by casino. Seeded once with the first casino, then left to the reader. */
  protected readonly expanded = linkedSignal<readonly PublishedOffer[], ReadonlySet<string>>({
    source: this.offers,
    computation: (offers, previous) =>
      previous?.value ?? new Set(offers.length > 0 ? [offers[0].casino] : []),
  });

  protected readonly wagering = wagering;
  protected readonly host = host;
  protected readonly initial = initial;
  protected readonly provenance = provenance;

  protected isOpen(offer: PublishedOffer): boolean {
    return this.expanded().has(offer.casino);
  }

  protected toggle(offer: PublishedOffer): void {
    this.expanded.update((open) => {
      const next = new Set(open);
      if (!next.delete(offer.casino)) {
        next.add(offer.casino);
      }
      return next;
    });
  }

  protected panelId(index: number): string {
    return `offer-detail-${index}`;
  }

  protected tiles(offer: PublishedOffer): readonly FigureTile[] {
    const { matchPercent, maxBonus, freeSpins } = offer.details;
    return [
      {
        label: 'Match',
        value: matchPercent === null ? 'None' : `${matchPercent}%`,
        muted: matchPercent === null,
      },
      { label: 'Max bonus', value: money(maxBonus), muted: false },
      { label: 'Free spins', value: freeSpins ? String(freeSpins) : 'None', muted: !freeSpins },
      { label: 'Wagering', value: wagering(offer), muted: false },
    ];
  }

  protected articleOnly(offer: PublishedOffer): boolean {
    return (
      offer.verification.status === 'confirmed' && offer.verification.basis === 'reviewArticle'
    );
  }
}

function money(value: PublishedMoney | null): string {
  return value === null ? 'Not stated' : `${value.amount} ${value.currency}`;
}
