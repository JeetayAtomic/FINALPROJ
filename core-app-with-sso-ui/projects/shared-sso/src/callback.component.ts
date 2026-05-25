import { Component, Inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { APP_CONFIG, SampleAppConfig } from './app-config';
import { SessionService } from './session.service';

@Component({
  selector: 'app-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="shell">
      <div class="panel">
        <div class="spinner" *ngIf="!error()"></div>
        <ng-container *ngIf="!error(); else errTpl">
          <h1>Signing you in to {{ config.name }}…</h1>
          <p class="muted">Validating SSO token with the Dashboard.</p>
        </ng-container>
        <ng-template #errTpl>
          <h1>Access denied</h1>
          <p class="muted">{{ error() }}</p>
          <p class="muted small">Return to the Dashboard and click the tile again — SSO tokens are one-time use and expire after 5 minutes.</p>
        </ng-template>
      </div>
    </div>
  `,
  styles: [`
    .shell { min-height: 100vh; display: flex; align-items: center; justify-content: center; background: #f5f6fa; padding: 16px; }
    .panel { background: white; padding: 32px 40px; border-radius: 10px; box-shadow: 0 2px 8px rgba(0,0,0,0.06); text-align: center; max-width: 440px; width: 100%; }
    h1 { margin: 12px 0 6px; font-size: 20px; color: #202124; }
    .muted { color: #5f6368; margin: 4px 0; }
    .small { font-size: 12px; }
    .spinner {
      width: 36px; height: 36px; border: 4px solid #e0e0e0; border-top-color: #4285F4;
      border-radius: 50%; margin: 0 auto 8px; animation: spin 0.9s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
  `]
})
export class CallbackComponent implements OnInit {
  error = signal('');

  constructor(
    @Inject(APP_CONFIG) public config: SampleAppConfig,
    private route: ActivatedRoute,
    private router: Router,
    private session: SessionService
  ) {}

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!token) {
      this.error.set('Missing SSO token in query string.');
      return;
    }
    this.session.validate(token).subscribe({
      next: res => {
        if (res.isValid) {
          this.router.navigate(['/home']);
        } else {
          this.error.set('Token was rejected by the Dashboard (expired, already used, or invalid).');
        }
      },
      error: () => this.error.set('Failed to reach the Dashboard SSO service.')
    });
  }
}
