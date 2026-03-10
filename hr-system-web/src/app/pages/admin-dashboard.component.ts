import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { HrApiService } from '../core/services/hr-api.service';
import { AdminDashboard, JobApplication } from '../core/models/api.models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, BaseChartDirective],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class AdminDashboardComponent implements OnInit {
  private readonly api = inject(HrApiService);

  dashboard: AdminDashboard | null = null;
  applications: JobApplication[] = [];
  selectedApplicationId: number | null = null;
  reviewStage = 'UnderReview';
  reviewTestScore: number | null = null;
  reviewReply = '';
  reviewMessage = '';
  reviewError = '';

  lineData: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [] };
  clusteredBarData: ChartConfiguration<'bar'>['data'] = { labels: [], datasets: [] };

  lineOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false
  };

  barOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: {
        stacked: false
      },
      y: {
        beginAtZero: true
      }
    },
    plugins: {
      legend: { display: true }
    }
  };

  ngOnInit(): void {
    this.load();
  }

  get selectedApplication(): JobApplication | null {
    return this.applications.find((application) => application.id === this.selectedApplicationId) ?? null;
  }

  load() {
    this.api.getAdminDashboard().subscribe((data) => {
      this.dashboard = data;
      this.lineData = {
        labels: data.jobPostsByMonth.map((x) => x.label),
        datasets: [
          {
            label: 'Jobs Posted',
            data: data.jobPostsByMonth.map((x) => x.value),
            borderColor: '#2563eb',
            backgroundColor: 'rgba(37, 99, 235, 0.14)',
            tension: 0.3,
            fill: true
          },
          {
            label: 'Applications',
            data: data.applicationsByMonth.map((x) => x.value),
            borderColor: '#16a34a',
            backgroundColor: 'rgba(22, 163, 74, 0.12)',
            tension: 0.3,
            fill: true
          }
        ]
      };

      this.clusteredBarData = {
        labels: data.jobPostsByMonth.map((x) => x.label),
        datasets: [
          {
            label: 'Jobs Posted',
            data: data.jobPostsByMonth.map((x) => x.value),
            backgroundColor: 'rgba(37, 99, 235, 0.82)',
            borderRadius: 8,
            barPercentage: 0.72
          },
          {
            label: 'Applications',
            data: data.applicationsByMonth.map((x) => x.value),
            backgroundColor: 'rgba(22, 163, 74, 0.82)',
            borderRadius: 8,
            barPercentage: 0.72
          }
        ]
      };
    });

    this.api.getAllApplications().subscribe((applications) => {
      this.applications = applications;
      if (!this.selectedApplicationId && applications.length > 0) {
        this.selectApplication(applications[0]);
      }
    });
  }

  selectApplication(application: JobApplication) {
    this.selectedApplicationId = application.id;
    this.reviewStage = application.stage;
    this.reviewTestScore = application.testScore ?? null;
    this.reviewReply = application.adminReply ?? '';
    this.reviewMessage = '';
    this.reviewError = '';
  }

  submitReview() {
    if (!this.selectedApplication) {
      return;
    }

    const reply = this.reviewReply.trim();
    if (!reply) {
      this.reviewError = 'A reply to the candidate is required.';
      return;
    }

    this.api.reviewApplication({
      applicationId: this.selectedApplication.id,
      stage: this.reviewStage,
      testScore: this.reviewTestScore === null ? undefined : this.reviewTestScore,
      reply
    }).subscribe({
      next: (application) => {
        this.reviewMessage = 'Candidate review saved and notification sent.';
        this.reviewError = '';
        const index = this.applications.findIndex((item) => item.id === application.id);
        if (index >= 0) {
          this.applications[index] = application;
        }
        this.selectApplication(application);
      },
      error: (err) => {
        this.reviewError = err?.error?.message ?? 'Unable to save candidate review.';
        this.reviewMessage = '';
      }
    });
  }
}
