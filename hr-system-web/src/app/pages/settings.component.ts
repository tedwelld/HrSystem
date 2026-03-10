import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../core/services/auth.service';
import { PdfExportService } from '../core/services/pdf-export.service';
import { HrApiService } from '../core/services/hr-api.service';
import { UiPreferenceService } from '../core/services/ui-preference.service';
import {
  AdminCompany,
  AdminUser,
  CreateAdminCompanyRequest,
  CreateHrAdminRequest,
  UserProfile,
  UserPreference
} from '../core/models/api.models';

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
  private readonly pdfExport = inject(PdfExportService);
  private readonly preferenceService = inject(UiPreferenceService);

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
  candidateSearch = '';
  sendToAllUsers = false;
  sendToAllCandidates = false;
  emailSubject = '';
  emailMessage = '';
  hrAdminDraft: CreateHrAdminRequest = this.createEmptyHrAdminDraft();
  companyDraft: CreateAdminCompanyRequest = this.createEmptyCompanyDraft();

  loading = false;
  error = '';
  success = '';

  ngOnInit(): void {
    this.loadBase();
  }

  get candidateUsers() {
    const query = this.candidateSearch.trim().toLowerCase();
    return this.users.filter(
      (user) =>
        user.role === 'Candidate' &&
        (!query ||
          `${user.firstName} ${user.lastName}`.toLowerCase().includes(query) ||
          user.email.toLowerCase().includes(query) ||
          user.phoneNumber.toLowerCase().includes(query))
    );
  }

  get hrAdminUsers() {
    return this.users.filter((user) => user.role === 'Admin');
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
      this.preferenceService.applyPreference(pref);
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
          this.preferenceService.applyPreference(updated);
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

  createHrAdmin() {
    this.api.createHrAdmin(this.hrAdminDraft).subscribe({
      next: (created) => {
        this.users = [created, ...this.users];
        this.hrAdminDraft = this.createEmptyHrAdminDraft();
        this.success = `Created HR admin ${created.email}.`;
        this.error = '';
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Could not create HR admin.';
        this.success = '';
      }
    });
  }

  deleteUser(user: AdminUser) {
    if (!window.confirm(`Deactivate ${user.email}?`)) {
      return;
    }

    this.api.deleteAdminUser(user.id).subscribe({
      next: () => {
        this.users = this.users.map((item) => (item.id === user.id ? { ...item, isActive: false } : item));
        this.selectedUserIds = this.selectedUserIds.filter((id) => id !== user.id);
        this.success = `${user.email} deactivated.`;
        this.error = '';
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Could not deactivate user.';
        this.success = '';
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

  createCompany() {
    this.api.createAdminCompany(this.companyDraft).subscribe({
      next: (created) => {
        this.companies = [created, ...this.companies];
        this.companyDraft = this.createEmptyCompanyDraft();
        this.success = `Created company ${created.name}.`;
        this.error = '';
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Could not create company.';
        this.success = '';
      }
    });
  }

  deleteCompany(company: AdminCompany) {
    if (!window.confirm(`Delete company ${company.name}?`)) {
      return;
    }

    this.api.deleteAdminCompany(company.id).subscribe({
      next: () => {
        this.companies = this.companies.filter((item) => item.id !== company.id);
        this.success = `Deleted company ${company.name}.`;
        this.error = '';
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Could not delete company.';
        this.success = '';
      }
    });
  }

  sendEmail() {
    const subject = this.emailSubject.trim();
    const message = this.emailMessage.trim();

    if (!subject || !message) {
      this.error = 'Subject and message are required.';
      this.success = '';
      return;
    }

    if (!this.sendToAllUsers && !this.sendToAllCandidates && this.selectedUserIds.length === 0) {
      this.error = 'Select at least one recipient, or choose all candidates/all users.';
      this.success = '';
      return;
    }

    this.api
      .sendAdminEmail({
        userIds: this.selectedUserIds,
        includeAllUsers: this.sendToAllUsers,
        includeAllCandidates: this.sendToAllCandidates,
        subject,
        message
      })
      .subscribe({
        next: (result) => {
          this.success = `Emails sent: ${result.successfullySent}/${result.requestedRecipients}, failed: ${result.failed}.`;
          this.error = '';
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Could not send emails.';
          this.success = '';
        }
      });
  }

  onAllUsersChanged(checked: boolean) {
    this.sendToAllUsers = checked;
    if (checked) {
      this.sendToAllCandidates = false;
    }
  }

  onAllCandidatesChanged(checked: boolean) {
    this.sendToAllCandidates = checked;
    if (checked) {
      this.sendToAllUsers = false;
    }
  }

  toggleRecipient(userId: number, checked: boolean) {
    if (checked) {
      this.selectedUserIds = [...new Set([...this.selectedUserIds, userId])];
      return;
    }

    this.selectedUserIds = this.selectedUserIds.filter((x) => x !== userId);
  }

  trackUser(_: number, user: AdminUser) {
    return user.id;
  }

  trackCompany(_: number, company: AdminCompany) {
    return company.id;
  }

  exportCandidatesPdf() {
    void this.pdfExport.exportTable(
      'Registered Candidates',
      'registered-candidates.pdf',
      ['Name', 'Email', 'Phone', 'Role', 'Active', 'Registered'],
      this.candidateUsers.map((user) => [
        `${user.firstName} ${user.lastName}`,
        user.email,
        user.phoneNumber || '-',
        user.role,
        user.isActive ? 'Yes' : 'No',
        new Date(user.createdAtUtc).toLocaleString()
      ])
    );
  }

  private createEmptyHrAdminDraft(): CreateHrAdminRequest {
    return {
      firstName: '',
      lastName: '',
      email: '',
      phoneNumber: '',
      password: ''
    };
  }

  private createEmptyCompanyDraft(): CreateAdminCompanyRequest {
    return {
      name: '',
      address: '',
      city: '',
      country: '',
      phone: '',
      email: '',
      description: ''
    };
  }
}
