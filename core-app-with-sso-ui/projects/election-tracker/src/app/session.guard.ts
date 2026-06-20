import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { ElectionService } from './election.service';

// Gate the tracker: ask the backend whether the current cookie maps to an active
// session for THIS app. No valid session => bounce to the denied page.
export const sessionGuard: CanActivateFn = () => {
  const svc = inject(ElectionService);
  const router = inject(Router);
  return svc.getSession().pipe(
    map(() => true),
    catchError(() => of(router.createUrlTree(['/denied']))),
  );
};
