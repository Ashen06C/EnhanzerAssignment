import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import {
  AllowedItem,
  LocationOption,
  PurchaseItemDraft,
  allowedItems
} from '../../../../core/models/purchase-bill.models';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner.component';

type PurchaseItemForm = FormGroup<{
  itemName: FormControl<string>;
  locationCode: FormControl<string>;
  standardCost: FormControl<number | null>;
  standardPrice: FormControl<number | null>;
  quantity: FormControl<number | null>;
  discountPercentage: FormControl<number | null>;
}>;

const allowedItemValidator = (control: AbstractControl<string>): ValidationErrors | null => {
  const value = control.value.toLocaleLowerCase();
  return allowedItems.some((item) => item.toLocaleLowerCase() === value) ? null : { allowedItem: true };
};

@Component({
  selector: 'app-purchase-item-form',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatOptionModule,
    MatSelectModule,
    LoadingSpinnerComponent
  ],
  templateUrl: './purchase-item-form.component.html',
  styleUrl: './purchase-item-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PurchaseItemFormComponent {
  readonly locations = input.required<LocationOption[]>();
  readonly locationsLoading = input(false);
  readonly locationsError = input(false);
  readonly addItem = output<PurchaseItemDraft>();
  readonly retryLocations = output<void>();

  readonly allItems = allowedItems;
  readonly form: PurchaseItemForm = new FormGroup({
    itemName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, allowedItemValidator]
    }),
    locationCode: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required]
    }),
    standardCost: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]),
    standardPrice: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]),
    quantity: new FormControl<number | null>(null, [Validators.required, Validators.min(0.01)]),
    discountPercentage: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(0),
      Validators.max(100)
    ])
  });

  filteredItems(): readonly AllowedItem[] {
    const term = this.form.controls.itemName.value.toLocaleLowerCase();
    return this.allItems.filter((item) => item.toLocaleLowerCase().includes(term));
  }

  onAutocompleteSelected(event: MatAutocompleteSelectedEvent): void {
    this.form.controls.itemName.setValue(event.option.value as string);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const selectedLocation = this.locations().find((location) => location.locationCode === value.locationCode);
    const matchedItem = allowedItems.find(
      (item) => item.toLocaleLowerCase() === value.itemName.toLocaleLowerCase()
    );

    if (!selectedLocation || !matchedItem) {
      this.form.markAllAsTouched();
      return;
    }

    this.addItem.emit({
      itemName: matchedItem as AllowedItem,
      locationCode: selectedLocation.locationCode,
      locationName: selectedLocation.locationName,
      standardCost: value.standardCost ?? 0,
      standardPrice: value.standardPrice ?? 0,
      quantity: value.quantity ?? 0,
      discountPercentage: value.discountPercentage ?? 0
    });

    this.form.reset({
      itemName: '',
      locationCode: value.locationCode,
      standardCost: null,
      standardPrice: null,
      quantity: null,
      discountPercentage: null
    });
  }
}
