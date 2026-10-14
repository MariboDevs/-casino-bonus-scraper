import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { PublishedOffer } from '../offers/published-offers';
import { VerificationBadge } from '../verification-badge/verification-badge';

/** The casinos table. Below the small breakpoint each row reflows into a labelled card. */
@Component({
  selector: 'app-offer-table',
  imports: [VerificationBadge],
  templateUrl: './offer-table.html',
  styleUrl: './offer-table.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OfferTable {
  readonly offers = input.required<readonly PublishedOffer[]>();

  protected wagering(offer: PublishedOffer): string {
    const multiplier = offer.wageringRequirement;
    if (multiplier === null) {
      return 'Not stated';
    }

    return multiplier === 0 ? 'No wagering' : `${multiplier}x`;
  }

  protected host(url: string): string {
    return new URL(url).hostname.replace(/^www\./, '');
  }
}
