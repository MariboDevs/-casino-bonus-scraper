import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { summary } from '../offers/offer-view';
import { PublishedOffers } from '../offers/published-offers';

/** The four-figure strip above the offers. */
@Component({
  selector: 'app-offer-summary',
  templateUrl: './offer-summary.html',
  styleUrl: './offer-summary.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OfferSummary {
  readonly published = input.required<PublishedOffers>();

  protected readonly cells = computed(() => summary(this.published()));
}
