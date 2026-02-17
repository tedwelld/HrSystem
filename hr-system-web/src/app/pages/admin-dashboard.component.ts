import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { HrApiService } from '../core/services/hr-api.service';
import { AdminDashboard } from '../core/models/api.models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, BaseChartDirective],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class AdminDashboardComponent implements OnInit {
  private readonly api = inject(HrApiService);

  dashboard: AdminDashboard | null = null;

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

  load() {
    this.api.getAdminDashboard().subscribe((data) => {
      this.dashboard = data;
      this.lineData = {
        labels: data.jobPostsByMonth.map((x) => x.label),
        datasets: [
          {
            label: 'Jobs Posted',
            data: data.jobPostsByMonth.map((x) => x.value),
            borderColor: '#0f766e',
            backgroundColor: 'rgba(15, 118, 110, 0.18)',
            tension: 0.3,
            fill: true
          },
          {
            label: 'Applications',
            data: data.applicationsByMonth.map((x) => x.value),
            borderColor: '#ea580c',
            backgroundColor: 'rgba(234, 88, 12, 0.1)',
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
            backgroundColor: 'rgba(20, 184, 166, 0.82)',
            borderRadius: 8,
            barPercentage: 0.72
          },
          {
            label: 'Applications',
            data: data.applicationsByMonth.map((x) => x.value),
            backgroundColor: 'rgba(59, 130, 246, 0.82)',
            borderRadius: 8,
            barPercentage: 0.72
          }
        ]
      };
    });
  }
}
