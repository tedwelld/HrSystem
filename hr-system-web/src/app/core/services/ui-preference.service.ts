import { computed, inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs/operators';
import { HrApiService } from './hr-api.service';
import { UserPreference } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class UiPreferenceService {
  private readonly api = inject(HrApiService);

  private readonly preferenceSignal = signal<UserPreference>({
    userId: 0,
    theme: 'light',
    autoHideSidebar: true,
    updatedAtUtc: new Date().toISOString()
  });

  readonly preference = computed(() => this.preferenceSignal());

  load() {
    return this.api.getMyPreference().pipe(
      tap({
        next: (preference) => this.applyPreference(preference),
        error: () => this.applyTheme(this.preferenceSignal().theme)
      })
    );
  }

  update(payload: { theme: 'light' | 'dark'; autoHideSidebar: boolean }) {
    return this.api.updateMyPreference(payload).pipe(tap((preference) => this.applyPreference(preference)));
  }

  applyPreference(preference: UserPreference) {
    this.preferenceSignal.set(preference);
    this.applyTheme(preference.theme);
  }

  applyTheme(theme: 'light' | 'dark') {
    document.body.setAttribute('data-theme', theme);
  }
}
