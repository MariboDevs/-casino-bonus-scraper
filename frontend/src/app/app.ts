import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { MethodSteps } from './method-steps/method-steps';
import { OfferSummary } from './offer-summary/offer-summary';
import { OfferTable } from './offer-table/offer-table';
import { failureReason } from './offers/failure-reason';
import { modelFamily, runStatus, shortRunId } from './offers/offer-view';
import { OffersStore } from './offers/offers-store';
import { SUPPORTED_SCHEMA_VERSION } from './offers/published-offers';
import { SiteHeader } from './site-header/site-header';

@Component({
  selector: 'app-root',
  imports: [DatePipe, MethodSteps, OfferSummary, OfferTable, SiteHeader],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly store = inject(OffersStore);
  protected readonly failureReason = failureReason;
  protected readonly schemaVersion = SUPPORTED_SCHEMA_VERSION;

  /** The published data, or null while loading and on a load error. */
  protected readonly published = computed(() =>
    this.store.loading() || this.store.failed() ? null : this.store.offers(),
  );

  protected readonly runStatus = computed(() => {
    const published = this.published();
    return published ? runStatus(published) : null;
  });

  protected readonly shortRunId = computed(() => {
    const published = this.published();
    return published ? shortRunId(published.runId) : '';
  });

  protected readonly modelFamily = computed(() => {
    const published = this.published();
    return published ? modelFamily(published.casinos) : '';
  });

  protected readonly modelTitle = computed(() => {
    const models = new Set(this.published()?.casinos.map((c) => c.extractedBy) ?? []);
    return [...models].join(', ');
  });
}
