import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  firstName = '';
  lastName = '';
  email = '';
  phoneNumber = '';
  password = '';
  role = 'Candidate';
  adminInviteCode = '';

  error = '';
  loading = false;

  submit() {
    this.error = '';
    this.loading = true;

    this.authService
      .register({
        firstName: this.firstName,
        lastName: this.lastName,
        email: this.email,
        phoneNumber: this.phoneNumber,
        password: this.password,
        role: this.role,
        adminInviteCode: this.adminInviteCode || undefined
      })
      .subscribe({
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
          this.error = err?.error?.message ?? 'Registration failed';
        }
      });
  }
}
