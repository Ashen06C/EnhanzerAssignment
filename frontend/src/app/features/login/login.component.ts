import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../core/auth/auth.service';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSnackBarModule,
    LoadingSpinnerComponent
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  readonly hidePassword = signal(true);
  readonly submitting = signal(false);
  readonly loginError = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  async submit(): Promise<void> {
    this.loginError.set(null);

    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    const { email, password } = this.form.getRawValue();

    try {
      await this.authService.login({ email: email.trim(), password });
      await this.router.navigateByUrl('/purchase-bill');
    } catch (error) {
      const message = this.getLoginErrorMessage(error);
      this.loginError.set(message);
      this.snackBar.open(message, 'Dismiss', {
        duration: 5000,
        panelClass: 'error-snackbar'
      });
    } finally {
      this.submitting.set(false);
    }
  }

  private getLoginErrorMessage(error: unknown): string {
    if (!(error instanceof HttpErrorResponse)) {
      return 'Login failed. Check your credentials or try again later.';
    }

    if (error.status === 0) {
      return 'Cannot reach the login service. Check that the API is running.';
    }

    if (error.status === 400) {
      return 'Enter a valid email address and password.';
    }

    if (error.status === 401) {
      return 'The email or password was not accepted.';
    }

    if (error.status === 502) {
      return 'The external login service is unavailable. Please try again later.';
    }

    return 'Login failed. Check your credentials or try again later.';
  }
}
