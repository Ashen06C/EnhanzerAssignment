import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-loading-spinner',
  imports: [MatProgressSpinnerModule],
  template: `
    <span class="spinner" role="status" [attr.aria-label]="label()">
      <mat-progress-spinner diameter="22" mode="indeterminate" />
    </span>
  `,
  styles: `
    .spinner {
      display: inline-flex;
      align-items: center;
      justify-content: center;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoadingSpinnerComponent {
  readonly label = input('Loading');
}
