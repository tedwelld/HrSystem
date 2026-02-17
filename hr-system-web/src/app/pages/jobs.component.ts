import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../core/services/auth.service';
import { HrApiService } from '../core/services/hr-api.service';
import { CvProfile, JobPosting } from '../core/models/api.models';

interface JobFormState {
  title: string;
  description: string;
  location: string;
  employmentType: string;
  experienceLevel: string;
  requiredSkillsCsv: string;
  salaryMin?: number;
  salaryMax?: number;
  companyId: number;
}

@Component({
  selector: 'app-jobs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './jobs.component.html',
  styleUrl: './jobs.component.scss'
})
export class JobsComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly api = inject(HrApiService);

  readonly role = computed(() => this.authService.user()?.role ?? 'Candidate');

  readonly titleOptions = [
    'Backend Engineer',
    'Frontend Engineer',
    '.NET Developer',
    'HR Specialist',
    'Data Analyst',
    'Project Manager'
  ];

  readonly locationOptions = [
    'Harare, Zimbabwe',
    'Bulawayo, Zimbabwe',
    'Mutare, Zimbabwe',
    'Gweru, Zimbabwe',
    'Victoria Falls, Zimbabwe',
    'Remote - Zimbabwe'
  ];
  readonly employmentTypeOptions = ['Full-time', 'Part-time', 'Contract', 'Internship'];
  readonly experienceLevelOptions = ['Junior', 'Mid', 'Senior', 'Lead'];
  readonly skillPresets = [
    { label: '.NET API', skills: ['c#', 'dotnet', 'sql', 'rest api'] },
    { label: 'Frontend Angular', skills: ['angular', 'typescript', 'html', 'css'] },
    { label: 'Cloud/DevOps', skills: ['azure', 'docker', 'kubernetes', 'git'] },
    { label: 'HR Operations', skills: ['recruitment', 'onboarding', 'communication', 'hris'] }
  ];

  jobs: JobPosting[] = [];
  cvProfiles: CvProfile[] = [];

  loading = false;
  error = '';
  success = '';

  selectedCvId?: number;
  coverLetter = '';

  newJob: JobFormState = this.createDefaultJob();
  editJob: JobFormState = this.createDefaultJob();

  selectedNewPreset = this.skillPresets[0].label;
  selectedEditPreset = this.skillPresets[0].label;
  editingJobId: number | null = null;

  structuredCv = {
    fileName: 'candidate-cv.json',
    fullText: '',
    skillsCsv: '',
    educationSummary: '',
    yearsOfExperience: 0,
    certificationsSummary: ''
  };

  selectedFile: File | null = null;

  ngOnInit(): void {
    this.applyPreset('new', this.selectedNewPreset);
    this.load();
  }

  load() {
    this.loading = true;
    this.error = '';

    const jobsRequest = this.role() === 'Admin' ? this.api.getAllJobs() : this.api.getOpenJobs();

    jobsRequest.subscribe({
      next: (jobs) => {
        this.jobs = jobs;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? 'Failed to load jobs.';
      }
    });

    if (this.role() === 'Candidate') {
      this.loadCvProfiles();
    }
  }

  loadCvProfiles() {
    this.api.getCvProfiles().subscribe((profiles) => {
      this.cvProfiles = profiles;
      if (!this.selectedCvId && profiles.length > 0) {
        this.selectedCvId = profiles[0].id;
      }
    });
  }

  resetCreateForm() {
    this.newJob = this.createDefaultJob();
    this.selectedNewPreset = this.skillPresets[0].label;
    this.applyPreset('new', this.selectedNewPreset);
    this.success = 'New job form added and reset.';
  }

  createJob() {
    this.success = '';
    this.error = '';

    this.api
      .createJob({
        ...this.newJob,
        requiredSkills: this.parseSkills(this.newJob.requiredSkillsCsv)
      })
      .subscribe({
        next: () => {
          this.success = 'Vacancy posted successfully.';
          this.resetCreateForm();
          this.load();
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Unable to post vacancy.';
        }
      });
  }

  startEdit(job: JobPosting) {
    this.editingJobId = job.id;
    this.editJob = {
      title: job.title,
      description: job.description,
      location: job.location,
      employmentType: job.employmentType,
      experienceLevel: job.experienceLevel,
      requiredSkillsCsv: job.requiredSkills.join(', '),
      salaryMin: job.salaryMin,
      salaryMax: job.salaryMax,
      companyId: job.companyId
    };
    this.success = `Editing job #${job.id}`;
  }

  cancelEdit() {
    this.editingJobId = null;
    this.editJob = this.createDefaultJob();
    this.success = 'Edit canceled.';
  }

  saveEdit() {
    if (!this.editingJobId) {
      return;
    }

    this.api
      .updateJob(this.editingJobId, {
        ...this.editJob,
        requiredSkills: this.parseSkills(this.editJob.requiredSkillsCsv)
      })
      .subscribe({
        next: () => {
          this.success = `Job #${this.editingJobId} updated.`;
          this.editingJobId = null;
          this.load();
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Unable to update job.';
        }
      });
  }

  deleteJob(jobId: number) {
    this.api.deleteJob(jobId).subscribe({
      next: () => {
        this.success = `Job #${jobId} deleted.`;
        this.load();
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Unable to delete job.';
      }
    });
  }

  closeJob(jobId: number) {
    this.api.closeJob(jobId).subscribe({
      next: () => {
        this.success = `Job #${jobId} closed.`;
        this.load();
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Unable to close job.';
      }
    });
  }

  reopenJob(job: JobPosting) {
    this.api.updateJob(job.id, { isOpen: true }).subscribe({
      next: () => {
        this.success = `Job #${job.id} reopened.`;
        this.load();
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Unable to reopen job.';
      }
    });
  }

  apply(jobId: number) {
    this.success = '';
    this.error = '';

    this.api
      .applyForJob({
        jobPostingId: jobId,
        cvProfileId: this.selectedCvId,
        coverLetter: this.coverLetter
      })
      .subscribe({
        next: () => {
          this.success = 'Application sent successfully.';
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Application failed.';
        }
      });
  }

  uploadStructuredCv() {
    this.success = '';
    this.error = '';

    this.api
      .uploadStructuredCv({
        fileName: this.structuredCv.fileName,
        fullText: this.structuredCv.fullText,
        skills: this.structuredCv.skillsCsv
          .split(',')
          .map((x) => x.trim())
          .filter(Boolean),
        educationSummary: this.structuredCv.educationSummary,
        yearsOfExperience: this.structuredCv.yearsOfExperience,
        certificationsSummary: this.structuredCv.certificationsSummary
      })
      .subscribe({
        next: () => {
          this.success = 'Structured CV uploaded.';
          this.loadCvProfiles();
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Structured CV upload failed.';
        }
      });
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
  }

  uploadTextCv() {
    if (!this.selectedFile) {
      this.error = 'Select a file first.';
      return;
    }

    this.api.uploadTextCv(this.selectedFile).subscribe({
      next: () => {
        this.success = 'Text CV uploaded.';
        this.selectedFile = null;
        this.loadCvProfiles();
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'File upload failed.';
      }
    });
  }

  applyPreset(mode: 'new' | 'edit', presetLabel: string) {
    const preset = this.skillPresets.find((x) => x.label === presetLabel);
    if (!preset) {
      return;
    }

    const csv = preset.skills.join(', ');
    if (mode === 'new') {
      this.newJob.requiredSkillsCsv = csv;
      return;
    }

    this.editJob.requiredSkillsCsv = csv;
  }

  private createDefaultJob(): JobFormState {
    return {
      title: this.titleOptions[0],
      description: '',
      location: this.locationOptions[0],
      employmentType: this.employmentTypeOptions[0],
      experienceLevel: this.experienceLevelOptions[1],
      requiredSkillsCsv: '',
      salaryMin: undefined,
      salaryMax: undefined,
      companyId: 1
    };
  }

  private parseSkills(csv: string): string[] {
    return csv
      .split(',')
      .map((x) => x.trim())
      .filter(Boolean);
  }
}
