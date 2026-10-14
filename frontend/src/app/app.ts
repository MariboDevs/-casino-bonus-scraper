import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { OfferTable } from './offer-table/offer-table';
import { OffersStore } from './offers/offers-store';

@Component({
  selector: 'app-root',
  imports: [DatePipe, OfferTable],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly store = inject(OffersStore);
}
