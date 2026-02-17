import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../core/services/auth.service';
import { HrApiService } from '../core/services/hr-api.service';
import { AdminCompany, AdminUser, UserProfile, UserPreference } from '../core/models/api.models';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent implements OnInit {
  private readonly api = inject(HrApiService);
  private readonly auth = inject(AuthService);

  readonly isAdmin = computed(() => this.auth.isAdmin());

  profile: UserProfile | null = null;
  preferences: UserPreference = {
    userId: 0,
    theme: 'light',
    autoHideSidebar: true,
    updatedAtUtc: new Date().toISOString()
  };

  users: AdminUser[] = [];
  companies: AdminCompany[] = [];
  selectedUserIds: number[] = [];
  sendToAllCandidates = false;
  emailSubject = '';
  emailMessage = '';

  loading = false;
  error = '';
  success = '';

  ngOnInit(): void {
    this.loadBase();
  }

  loadBase() {
    this.loading = true;
    this.error = '';

    this.api.getMyProfile().subscribe({
      next: (profile) => {
        this.profile = profile;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? 'Could not load profile.';
      }
    });

    this.api.getMyPreference().subscribe((pref) => {
      this.preferences = pref;
    });

    if (this.isAdmin()) {
      this.loadAdminSettings();
    }
  }

  loadAdminSettings() {
    this.api.getAdminUsers().subscribe({
      next: (users) => {
        this.users = users;
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Failed loading users.';
      }
    });

    this.api.getAdminCompanies().subscribe({
      next: (companies) => {
        this.companies = companies;
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Failed loading companies.';
      }
    });
  }

  saveProfile() {
    if (!this.profile) {
      return;
    }

    this.api
      .updateMyProfile({
        firstName: this.profile.firstName,
        lastName: this.profile.lastName,
        phoneNumber: this.profile.phoneNumber
      })
      .subscribe({
        next: (updated) => {
          this.profile = updated;
          this.auth.updateCachedUser(updated);
          this.success = 'Profile updated.';
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Could not update profile.';
        }
      });
  }

  savePreferences() {
    this.api
      .updateMyPreference({
        theme: this.preferences.theme,
        autoHideSidebar: this.preferences.autoHideSidebar
      })
      .subscribe({
        next: (updated) => {
          this.preferences = updated;
          document.body.setAttribute('data-theme', updated.theme);
          this.success = 'Preferences saved.';
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Could not update preferences.';
        }
      });
  }

  saveUser(user: AdminUser) {
    this.api
      .updateAdminUser(user.id, {
        firstName: user.firstName,
        lastName: user.lastName,
        phoneNumber: user.phoneNumber,
        role: user.role,
        isActive: user.isActive
      })
      .subscribe({
        next: (updated) => {
          const idx = this.users.findIndex((x) => x.id === updated.id);
          if (idx >= 0) {
            this.users[idx] = updated;
          }
          this.success = `Updated user ${updated.email}.`;
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Could not update user.';
        }
      });
  }

  saveCompany(company: AdminCompany) {
    this.api
      .updateAdminCompany(company.id, {
        name: company.name,
        address: company.address,
        city: company.city,
        country: company.country,
        phone: company.phone,
        email: company.email,
        description: company.description
      })
      .subscribe({
        next: (updated) => {
          const idx = this.companies.findIndex((x) => x.id === updated.id);
          if (idx >= 0) {
            this.companies[idx] = updated;
          }
          this.success = `Updated company ${updated.name}.`;
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Could not update company.';
        }
      });
  }

  sendEmail() {
    this.api
      .sendAdminEmail({
        userIds: this.selectedUserIds,
        includeAllCandidates: this.sendToAllCandidates,
        subject: this.emailSubject,
        message: this.emailMessage
      })
      .subscribe({
        next: (result) => {
          this.success = `Emails sent: ${result.successfullySent}/${result.requestedRecipients}, failed: ${result.failed}.`;
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Could not send emails.';
        }
      });
  }

  toggleRecipient(userId: number, checked: boolean) {
    if (checked) {
      this.selectedUserIds = [...new Set([...this.selectedUserIds, userId])];
      return;
    }

    this.selectedUserIds = this.selectedUserIds.filter((x) => x !== userId);
  }
}
