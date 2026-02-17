import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  email = '';
  password = '';
  error = '';
  loading = false;

  submit() {
    this.error = '';
    this.loading = true;

    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        this.loading = false;
        if (this.authService.isAdmin()) {
          this.router.navigate(['/app/admin-dashboard']);
          return;
        }

        this.router.navigate(['/app/candidate-dashboard']);
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? 'Login failed';
      }
    });
  }
}
