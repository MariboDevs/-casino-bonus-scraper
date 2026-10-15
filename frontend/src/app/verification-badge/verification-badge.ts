import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { badgeKind } from '../offers/offer-view';
import { PublishedVerification } from '../offers/published-offers';

/**
 * Says whether a deterministic re-reading of the source text agreed with the language
 * model. Display only: the reasons live in the table's expanded row and inline on cards.
 */
@Component({
  selector: 'app-verification-badge',
  templateUrl: './verification-badge.html',
  styleUrl: './verification-badge.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VerificationBadge {
  readonly verification = input.required<PublishedVerification>();

  /** Cards have less room, so the review-text label shortens. */
  readonly compact = input(false);

  protected readonly kind = computed(() => badgeKind(this.verification()));
}
