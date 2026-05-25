import { Routes } from '@angular/router';
import { CallbackComponent } from './callback.component';
import { HomeComponent } from './home.component';
import { LandingComponent } from './landing.component';
import { sessionGuard } from './session.guard';

export const sampleAppRoutes: Routes = [
  { path: '', component: LandingComponent },
  { path: 'sso/callback', component: CallbackComponent },
  { path: 'home', component: HomeComponent, canActivate: [sessionGuard] },
  { path: '**', redirectTo: '' }
];
