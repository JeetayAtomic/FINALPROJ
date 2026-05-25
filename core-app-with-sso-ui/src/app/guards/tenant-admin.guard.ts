import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const tenantAdminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    router.navigate(['/login']);
    return false;
  }

  const user = auth.currentUser();
  if (user?.isSuperAdmin) {
    router.navigate(['/admin/tenants']);
    return false;
  }

  if (user?.role !== 'Admin') {
    router.navigate(['/dashboard']);
    return false;
  }

  return true;
};
