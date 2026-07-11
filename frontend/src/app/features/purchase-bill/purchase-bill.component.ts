import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import {
  LocationOption,
  PurchaseBillLine,
  PurchaseItemDraft
} from '../../core/models/purchase-bill.models';
import { LocationService } from '../../core/services/location.service';
import { PurchaseBillApiService } from '../../core/services/purchase-bill-api.service';
import { AppHeaderComponent } from '../../shared/components/app-header/app-header.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { calculateLine, summarizeLines } from '../../shared/utilities/purchase-calculator';
import { ItemSummaryComponent } from './components/item-summary/item-summary.component';
import { PurchaseItemFormComponent } from './components/purchase-item-form/purchase-item-form.component';
import { PurchaseItemsTableComponent } from './components/purchase-items-table/purchase-items-table.component';

@Component({
  selector: 'app-purchase-bill',
  imports: [
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatSnackBarModule,
    AppHeaderComponent,
    ItemSummaryComponent,
    LoadingSpinnerComponent,
    PurchaseItemFormComponent,
    PurchaseItemsTableComponent
  ],
  templateUrl: './purchase-bill.component.html',
  styleUrl: './purchase-bill.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PurchaseBillComponent {
  private readonly authService = inject(AuthService);
  private readonly locationService = inject(LocationService);
  private readonly purchaseBillApi = inject(PurchaseBillApiService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  readonly user = this.authService.user;
  readonly locations = signal<LocationOption[]>([]);
  readonly locationsLoading = signal(false);
  readonly locationsError = signal(false);
  readonly lines = signal<PurchaseBillLine[]>([]);
  readonly saving = signal(false);
  readonly summary = computed(() => summarizeLines(this.lines()));

  constructor() {
    this.loadLocations();
  }

  loadLocations(): void {
    this.locationsLoading.set(true);
    this.locationsError.set(false);

    this.locationService
      .getLocations()
      .pipe(finalize(() => this.locationsLoading.set(false)))
      .subscribe({
        next: (locations) => this.locations.set(locations),
        error: () => {
          const cachedLocations = this.authService.user()?.locations ?? [];
          this.locations.set(cachedLocations);
          this.locationsError.set(cachedLocations.length === 0);
        }
      });
  }

  addItem(draft: PurchaseItemDraft): void {
    this.lines.update((lines) => [...lines, calculateLine(draft)]);
  }

  removeItem(id: string): void {
    this.lines.update((lines) => lines.filter((line) => line.id !== id));
  }

  saveBill(): void {
    if (this.lines().length === 0 || this.saving()) {
      return;
    }

    this.saving.set(true);
    this.purchaseBillApi
      .saveBill({ items: this.lines().map(({ id, grossCost, discountAmount, totalCost, totalSelling, ...draft }) => draft) })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (response) => {
          this.lines.set([]);
          this.snackBar.open(`Purchase bill saved: ${response.billId}`, 'Dismiss', {
            duration: 7000,
            panelClass: 'success-snackbar'
          });
        },
        error: () => {
          this.snackBar.open('Could not save the purchase bill. Your entered items were kept.', 'Dismiss', {
            duration: 6000,
            panelClass: 'error-snackbar'
          });
        }
      });
  }

  async logout(): Promise<void> {
    await this.authService.logout();
    await this.router.navigateByUrl('/login');
  }
}
