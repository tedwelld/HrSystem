import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject } from '@angular/core';
import { AuthService } from '../core/services/auth.service';
import { HrApiService } from '../core/services/hr-api.service';
import { Snapshot } from '../core/models/api.models';

@Component({
  selector: 'app-snapshots',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './snapshots.component.html',
  styleUrl: './snapshots.component.scss'
})
export class SnapshotsComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly api = inject(HrApiService);

  readonly isAdmin = computed(() => this.authService.isAdmin());

  snapshots: Snapshot[] = [];
  loading = false;
  error = '';

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.error = '';

    const request = this.isAdmin()
      ? this.api.getAdminSnapshots(200)
      : this.api.getMySnapshots(200);

    request.subscribe({
      next: (items) => {
        this.snapshots = items;
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Could not load snapshots.';
        this.loading = false;
      }
    });
  }
}
