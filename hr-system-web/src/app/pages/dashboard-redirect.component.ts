import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-dashboard-redirect',
  standalone: true,
  template: ''
})
export class DashboardRedirectComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  constructor() {
    if (this.authService.isAdmin()) {
      this.router.navigate(['/app/admin-dashboard']);
      return;
    }

    this.router.navigate(['/app/candidate-dashboard']);
  }
}
