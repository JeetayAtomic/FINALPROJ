import { Routes } from '@angular/router';
import { CallbackComponent } from './callback.component';
import { DashboardComponent } from './dashboard.component';
import { DeniedComponent } from './denied.component';
import { sessionGuard } from './session.guard';

export const routes: Routes = [
  // SSO entry point: the dashboard redirects the browser here with a one-time token.
  { path: 'sso/callback', component: CallbackComponent },
  { path: 'denied', component: DeniedComponent },
  // The tracker itself is gated — the guard only lets you in with a validated session.
  { path: '', component: DashboardComponent, canActivate: [sessionGuard] },
  { path: '**', redirectTo: '' },
];
