import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { PurchaseBillSummary } from '../../../../core/models/purchase-bill.models';

@Component({
  selector: 'app-item-summary',
  imports: [CurrencyPipe, DecimalPipe],
  templateUrl: './item-summary.component.html',
  styleUrl: './item-summary.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ItemSummaryComponent {
  readonly summary = input.required<PurchaseBillSummary>();
}
