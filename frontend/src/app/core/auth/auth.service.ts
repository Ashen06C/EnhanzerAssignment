import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthUser, LoginRequest } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly sessionStorageKey = 'enhanzer.auth.user';
  private readonly http = inject(HttpClient);
  private readonly userState = signal<AuthUser | null>(this.readStoredUser());
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
      this.setUser(user);
      return user;
    } catch {
      return this.userState();
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
    this.setUser(user);
    return user;
  }

  async logout(): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${environment.apiBaseUrl}/auth/logout`, {}));
    this.clearUser();
    this.hasAttemptedRestore = true;
  }

  private setUser(user: AuthUser): void {
    this.userState.set(user);
    sessionStorage.setItem(this.sessionStorageKey, JSON.stringify(user));
  }

  private clearUser(): void {
    this.userState.set(null);
    sessionStorage.removeItem(this.sessionStorageKey);
  }

  private readStoredUser(): AuthUser | null {
    const value = sessionStorage.getItem(this.sessionStorageKey);
    if (!value) {
      return null;
    }

    try {
      const user = JSON.parse(value) as Partial<AuthUser>;
      return typeof user.email === 'string' && Array.isArray(user.locations)
        ? { email: user.email, locations: user.locations }
        : null;
    } catch {
      sessionStorage.removeItem(this.sessionStorageKey);
      return null;
    }
  }
}
