import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    router.navigate(['/login']);
    return false;
  }

  // Super admins belong on /admin, not the tenant dashboard.
  if (auth.isSuperAdmin()) {
    router.navigate(['/admin/tenants']);
    return false;
  }

  return true;
};
