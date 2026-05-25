import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { APP_CONFIG, SampleAppConfig } from './app-config';
import { SessionService } from './session.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  template: `
    <header class="topbar" [style.background]="config.color">
      <div class="brand">
        <strong>{{ config.name }}</strong>
        <span class="tag">/{{ config.slug }}</span>
      </div>
      <div class="actions">
        <span class="user">{{ session.session()?.fullName }}</span>
        <button class="logout" (click)="logout()">Sign out</button>
      </div>
    </header>

    <main>
      <section class="card success">
        <div class="badge">SSO OK</div>
        <h1>Welcome to {{ config.name }}</h1>
        <p>Your identity was validated by the Dashboard SSO service.</p>

        <dl class="ident">
          <dt>Full name</dt><dd>{{ session.session()?.fullName }}</dd>
          <dt>Email</dt><dd>{{ session.session()?.email }}</dd>
          <dt>User&nbsp;id</dt><dd>{{ session.session()?.userId }}</dd>
          <dt>Tenant&nbsp;id</dt><dd>{{ session.session()?.tenantId }}</dd>
          <dt>Role</dt><dd><span class="pill">{{ session.session()?.role }}</span></dd>
          <dt>Session&nbsp;id</dt><dd class="mono">{{ session.session()?.sessionId }}</dd>
        </dl>

        <p class="muted small">Signing out here signs you out of <em>every</em> sample app — open {{ config.name }} in other tabs and watch them sign out within ~10 seconds.</p>
      </section>
    </main>
  `,
  styles: [`
    :host { display: block; min-height: 100vh; background: #f5f6fa; font-family: system-ui, -apple-system, Segoe UI, Roboto, sans-serif; }
    .topbar { color: white; padding: 14px 28px; display: flex; align-items: center; justify-content: space-between; }
    .brand strong { font-size: 16px; letter-spacing: 0.5px; }
    .brand .tag { margin-left: 10px; opacity: 0.7; font-size: 12px; }
    .actions { display: flex; align-items: center; gap: 16px; }
    .user { font-size: 13px; opacity: 0.92; }
    .logout { background: rgba(0,0,0,0.2); color: white; border: 1px solid rgba(255,255,255,0.25); padding: 6px 14px; border-radius: 6px; font-size: 13px; cursor: pointer; }
    .logout:hover { background: rgba(0,0,0,0.35); }
    main { max-width: 640px; margin: 40px auto; padding: 0 16px; }
    .card { background: white; border-radius: 10px; padding: 28px 32px; box-shadow: 0 2px 8px rgba(0,0,0,0.06); }
    .card h1 { margin: 6px 0; font-size: 24px; }
    .badge { display: inline-block; background: #e6f4ea; color: #137333; padding: 3px 10px; border-radius: 99px; font-size: 12px; margin-bottom: 8px; }
    dl.ident { display: grid; grid-template-columns: 120px 1fr; gap: 8px 16px; margin: 20px 0; padding: 16px; background: #f8fafc; border-radius: 8px; font-size: 14px; }
    dl.ident dt { color: #5f6368; font-weight: 500; }
    dl.ident dd { margin: 0; color: #202124; }
    dl.ident dd.mono { font-family: Consolas, monospace; font-size: 13px; }
    .pill { background: #e8f0fe; color: #1a73e8; padding: 2px 10px; border-radius: 10px; font-size: 12px; }
    .muted { color: #5f6368; }
    .small { font-size: 12px; }
  `]
})
export class HomeComponent {
  constructor(
    @Inject(APP_CONFIG) public config: SampleAppConfig,
    public session: SessionService
  ) {}

  logout(): void { this.session.logout(); }
}
