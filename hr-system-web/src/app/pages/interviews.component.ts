import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../core/services/auth.service';
import { HrApiService } from '../core/services/hr-api.service';
import { InterviewSchedule, JobApplication } from '../core/models/api.models';

@Component({
  selector: 'app-interviews',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './interviews.component.html',
  styleUrl: './interviews.component.scss'
})
export class InterviewsComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly api = inject(HrApiService);

  readonly role = computed(() => this.authService.user()?.role ?? 'Candidate');
  readonly statusOptions = ['Scheduled', 'Completed', 'Cancelled', 'NoShow'];
  readonly interviewTypes = ['Screening', 'Technical', 'Panel', 'HR', 'Final'];

  interviews: InterviewSchedule[] = [];
  applications: JobApplication[] = [];
  statusDraft: Record<number, string> = {};
  noteDraft: Record<number, string> = {};

  scheduleDraft = this.createScheduleDraft();

  loading = false;
  error = '';
  success = '';

  ngOnInit(): void {
    this.load();
  }

  get scheduleCandidates() {
    return this.applications.filter((application) => application.stage !== 'Rejected' && application.stage !== 'Offered');
  }

  load() {
    this.loading = true;
    this.error = '';

    const request = this.role() === 'Admin' ? this.api.getAdminInterviews() : this.api.getCandidateInterviews();
    request.subscribe({
      next: (interviews) => {
        this.interviews = interviews;
        this.statusDraft = {};
        this.noteDraft = {};
        interviews.forEach((interview) => {
          this.statusDraft[interview.id] = interview.status;
          this.noteDraft[interview.id] = interview.notes ?? '';
        });
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Failed to load interviews.';
        this.loading = false;
      }
    });

    if (this.role() === 'Admin') {
      this.api.getAllApplications().subscribe({
        next: (applications) => {
          this.applications = applications;
          if (!this.scheduleDraft.applicationId && applications.length > 0) {
            this.scheduleDraft.applicationId = applications[0].id;
          }
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Failed to load applications for interview scheduling.';
        }
      });
    }
  }

  scheduleInterview() {
    if (!this.scheduleDraft.applicationId) {
      this.error = 'Choose an application before scheduling the interview.';
      return;
    }

    if (!this.scheduleDraft.scheduledStartLocal || !this.scheduleDraft.scheduledEndLocal) {
      this.error = 'Both the start and end time are required.';
      return;
    }

    this.api
      .scheduleInterview({
        applicationId: this.scheduleDraft.applicationId,
        interviewType: this.scheduleDraft.interviewType,
        scheduledStartUtc: new Date(this.scheduleDraft.scheduledStartLocal).toISOString(),
        scheduledEndUtc: new Date(this.scheduleDraft.scheduledEndLocal).toISOString(),
        timeZone: this.scheduleDraft.timeZone,
        meetingLinkOrLocation: this.scheduleDraft.meetingLinkOrLocation,
        notes: this.scheduleDraft.notes
      })
      .subscribe({
        next: () => {
          this.success = 'Interview scheduled and candidate notified.';
          this.error = '';
          this.scheduleDraft = this.createScheduleDraft();
          this.load();
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Could not schedule the interview.';
          this.success = '';
        }
      });
  }

  updateStatus(interviewId: number) {
    this.api
      .updateInterviewStatus({
        interviewId,
        status: this.statusDraft[interviewId],
        notes: this.noteDraft[interviewId] ?? ''
      })
      .subscribe({
        next: () => {
          this.success = 'Interview updated.';
          this.error = '';
          this.load();
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Could not update the interview.';
          this.success = '';
        }
      });
  }

  trackInterview(_: number, interview: InterviewSchedule) {
    return interview.id;
  }

  trackApplication(_: number, application: JobApplication) {
    return application.id;
  }

  private createScheduleDraft() {
    return {
      applicationId: 0,
      interviewType: this.interviewTypes[0],
      scheduledStartLocal: '',
      scheduledEndLocal: '',
      timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC',
      meetingLinkOrLocation: '',
      notes: ''
    };
  }
}
