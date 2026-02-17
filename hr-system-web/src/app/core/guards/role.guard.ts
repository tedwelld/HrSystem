import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const requiredRole = route.data['role'] as string;
  const authService = inject(AuthService);
  const router = inject(Router);

  const user = authService.user();

  if (!user) {
    router.navigate(['/login']);
    return false;
  }

  if (requiredRole && user.role !== requiredRole) {
    router.navigate(['/app/jobs']);
    return false;
  }

  return true;
};
