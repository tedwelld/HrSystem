import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../core/services/auth.service';
import { HrApiService } from '../core/services/hr-api.service';
import { JobApplication } from '../core/models/api.models';

@Component({
  selector: 'app-applications',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './applications.component.html',
  styleUrl: './applications.component.scss'
})
export class ApplicationsComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly api = inject(HrApiService);

  readonly role = computed(() => this.authService.user()?.role ?? 'Candidate');

  applications: JobApplication[] = [];
  stageDraft: Record<number, string> = {};
  noteDraft: Record<number, string> = {};

  error = '';
  success = '';

  readonly stageOptions = [
    'Applied',
    'UnderReview',
    'Shortlisted',
    'InterviewScheduled',
    'Rejected',
    'Hired'
  ];

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.error = '';
    this.success = '';

    const request = this.role() === 'Admin' ? this.api.getAllApplications() : this.api.getMyApplications();
    request.subscribe({
      next: (data) => {
        this.applications = data;
        this.stageDraft = {};
        this.noteDraft = {};
        data.forEach((x) => {
          this.stageDraft[x.id] = x.stage;
          this.noteDraft[x.id] = '';
        });
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Could not load applications.';
      }
    });
  }

  updateStage(applicationId: number) {
    this.api
      .updateApplicationStage({
        applicationId,
        stage: this.stageDraft[applicationId]
      })
      .subscribe({
        next: () => {
          this.success = 'Application stage updated.';
          this.load();
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Stage update failed.';
        }
      });
  }

  addFollowUp(applicationId: number) {
    const note = this.noteDraft[applicationId]?.trim();
    if (!note) {
      this.error = 'Follow-up note cannot be empty.';
      return;
    }

    this.api.addFollowUp({ applicationId, note }).subscribe({
      next: () => {
        this.success = 'Follow-up note added.';
        this.noteDraft[applicationId] = '';
        this.load();
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Failed to add note.';
      }
    });
  }
}
