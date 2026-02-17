import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { HrApiService } from '../core/services/hr-api.service';
import { CandidateDashboard } from '../core/models/api.models';

@Component({
  selector: 'app-candidate-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, BaseChartDirective],
  templateUrl: './candidate-dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class CandidateDashboardComponent implements OnInit {
  private readonly api = inject(HrApiService);

  dashboard: CandidateDashboard | null = null;

  chartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [
      {
        label: 'My Applications',
        data: [],
        backgroundColor: '#0f766e'
      }
    ]
  };

  chartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false }
    }
  };

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.api.getCandidateDashboard().subscribe((data) => {
      this.dashboard = data;
      this.chartData = {
        labels: data.myApplicationsByStatus.map((x) => x.label),
        datasets: [
          {
            label: 'My Applications',
            data: data.myApplicationsByStatus.map((x) => x.value),
            backgroundColor: '#0f766e'
          }
        ]
      };
    });
  }
}
