import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { PublishedVerification } from '../offers/published-offers';

/**
 * Says whether a deterministic re-reading of the source text agreed with the language
 * model. Provisional offers expose the reasons, so the reader can judge for themselves.
 */
@Component({
  selector: 'app-verification-badge',
  templateUrl: './verification-badge.html',
  styleUrl: './verification-badge.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VerificationBadge {
  readonly verification = input.required<PublishedVerification>();

  protected readonly confirmed = computed(() => this.verification().status === 'confirmed');
}
