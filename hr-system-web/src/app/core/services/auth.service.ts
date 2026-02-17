import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthResponse, RegisterRequest, UserProfile } from '../models/api.models';

const TOKEN_KEY = 'hr_token';
const USER_KEY = 'hr_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly userSignal = signal<UserProfile | null>(this.loadUser());
  private readonly tokenSignal = signal<string>(localStorage.getItem(TOKEN_KEY) ?? '');

  readonly user = computed(() => this.userSignal());
  readonly token = computed(() => this.tokenSignal());
  readonly isAuthenticated = computed(() => !!this.tokenSignal());
  readonly isAdmin = computed(() => this.userSignal()?.role === 'Admin');
  readonly isCandidate = computed(() => this.userSignal()?.role === 'Candidate');

  login(email: string, password: string) {
    return this.http
      .post<AuthResponse>(`${environment.apiBaseUrl}/auth/login`, { email, password })
      .pipe(tap((response) => this.setSession(response)));
  }

  register(payload: RegisterRequest) {
    return this.http
      .post<AuthResponse>(`${environment.apiBaseUrl}/auth/register`, payload)
      .pipe(tap((response) => this.setSession(response)));
  }

  logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.userSignal.set(null);
    this.tokenSignal.set('');
    this.router.navigate(['/login']);
  }

  updateCachedUser(user: UserProfile) {
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this.userSignal.set(user);
  }

  private setSession(response: AuthResponse) {
    localStorage.setItem(TOKEN_KEY, response.token);
    localStorage.setItem(USER_KEY, JSON.stringify(response.user));
    this.tokenSignal.set(response.token);
    this.userSignal.set(response.user);
  }

  private loadUser(): UserProfile | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;

    try {
      return JSON.parse(raw) as UserProfile;
    } catch {
      return null;
    }
  }
}
