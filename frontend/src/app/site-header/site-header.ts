import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RunStatus } from '../offers/offer-view';

/** The pinned header: wordmark, run-status pill and the Download JSON action. */
@Component({
  selector: 'app-site-header',
  templateUrl: './site-header.html',
  styleUrl: './site-header.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SiteHeader {
  /** Null hides the pill (while loading and on a load error). */
  readonly runStatus = input<RunStatus>(null);
}
