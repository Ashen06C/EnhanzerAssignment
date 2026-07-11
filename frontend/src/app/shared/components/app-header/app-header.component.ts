import { ChangeDetectionStrategy, Component, output, input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-header',
  imports: [MatButtonModule, MatIconModule],
  template: `
    <header class="app-header">
      <div>
        <p class="eyebrow">Enhanzer Assignment</p>
        <h1>Purchase Bill</h1>
      </div>
      <div class="user-actions">
        <span>{{ email() }}</span>
        <button class="logout-button" mat-flat-button type="button" (click)="logout.emit()">
          <mat-icon>logout</mat-icon>
          Logout
        </button>
      </div>
    </header>
  `,
  styleUrl: './app-header.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppHeaderComponent {
  readonly email = input.required<string>();
  readonly logout = output<void>();
}
