import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../core/services/auth.service';
import { PdfExportService } from '../core/services/pdf-export.service';
import { HrApiService } from '../core/services/hr-api.service';
import { Snapshot } from '../core/models/api.models';

@Component({
  selector: 'app-snapshots',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './snapshots.component.html',
  styleUrl: './snapshots.component.scss'
})
export class SnapshotsComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly api = inject(HrApiService);
  private readonly pdfExport = inject(PdfExportService);

  readonly isAdmin = computed(() => this.authService.isAdmin());

  snapshots: Snapshot[] = [];
  searchTerm = '';
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

  trackSnapshot(_: number, snapshot: Snapshot) {
    return snapshot.id;
  }

  get filteredSnapshots() {
    const query = this.searchTerm.trim().toLowerCase();
    return this.snapshots.filter(
      (snapshot) =>
        !query ||
        snapshot.actorName.toLowerCase().includes(query) ||
        snapshot.source.toLowerCase().includes(query) ||
        snapshot.action.toLowerCase().includes(query) ||
        snapshot.category.toLowerCase().includes(query) ||
        snapshot.details.toLowerCase().includes(query)
    );
  }

  exportSnapshotsPdf() {
    void this.pdfExport.exportTable(
      'System Snapshots',
      'system-snapshots.pdf',
      ['Time', 'Actor', 'Source', 'Action', 'Category', 'Details'],
      this.filteredSnapshots.map((snapshot) => [
        new Date(snapshot.createdAtUtc).toLocaleString(),
        snapshot.actorName,
        snapshot.source,
        snapshot.action,
        snapshot.category,
        snapshot.details
      ])
    );
  }
}
