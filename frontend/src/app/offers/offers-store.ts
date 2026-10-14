import { httpResource } from '@angular/common/http';
import { Injectable, computed } from '@angular/core';
import { PublishedOffers, SUPPORTED_SCHEMA_VERSION } from './published-offers';

/** Loads the published offers once. Relative URL, so it works under a GitHub Pages sub-path. */
@Injectable({ providedIn: 'root' })
export class OffersStore {
  private readonly resource = httpResource<PublishedOffers>(() => 'data/casinos.json');

  readonly loading = this.resource.isLoading;
  readonly offers = computed(() => this.resource.value() ?? null);
  readonly unsupported = computed(() => {
    const offers = this.offers();
    return offers !== null && offers.schemaVersion !== SUPPORTED_SCHEMA_VERSION;
  });
  readonly failed = computed(() => this.resource.error() !== undefined || this.unsupported());
}
