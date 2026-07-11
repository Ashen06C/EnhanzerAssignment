import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthUser, LoginRequest } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly userState = signal<AuthUser | null>(null);
  private readonly restoringState = signal(false);
  private hasAttemptedRestore = false;

  readonly user = this.userState.asReadonly();
  readonly isAuthenticated = computed(() => this.userState() !== null);
  readonly isRestoring = this.restoringState.asReadonly();

  async restoreSession(): Promise<AuthUser | null> {
    if (this.hasAttemptedRestore) {
      return this.userState();
    }

    this.hasAttemptedRestore = true;
    this.restoringState.set(true);

    try {
      const user = await firstValueFrom(this.http.get<AuthUser>(`${environment.apiBaseUrl}/auth/me`));
      this.userState.set(user);
      return user;
    } catch {
      this.userState.set(null);
      return null;
    } finally {
      this.restoringState.set(false);
    }
  }

  async login(request: LoginRequest): Promise<AuthUser> {
    const user = await firstValueFrom(
      this.http.post<AuthUser>(`${environment.apiBaseUrl}/auth/login`, {
        ...request,
        email: request.email.trim()
      })
    );
    this.hasAttemptedRestore = true;
    this.userState.set(user);
    return user;
  }

  async logout(): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${environment.apiBaseUrl}/auth/logout`, {}));
    this.userState.set(null);
    this.hasAttemptedRestore = true;
  }
}
