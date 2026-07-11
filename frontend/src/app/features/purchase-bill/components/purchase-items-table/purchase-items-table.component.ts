import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { PurchaseBillLine } from '../../../../core/models/purchase-bill.models';

@Component({
  selector: 'app-purchase-items-table',
  imports: [CurrencyPipe, DecimalPipe, MatButtonModule, MatIconModule, MatTableModule],
  templateUrl: './purchase-items-table.component.html',
  styleUrl: './purchase-items-table.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PurchaseItemsTableComponent {
  readonly lines = input.required<PurchaseBillLine[]>();
  readonly remove = output<string>();

  readonly displayedColumns = [
    'itemName',
    'locationName',
    'standardCost',
    'standardPrice',
    'quantity',
    'discountPercentage',
    'totalCost',
    'totalSelling',
    'actions'
  ];
}
