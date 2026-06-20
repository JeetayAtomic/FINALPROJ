import { Component } from '@angular/core';
import { environment } from '../environments/environment';

@Component({
  selector: 'et-denied',
  template: `
    <main>
      <section class="card">
        <div class="badge">Access denied</div>
        <h1>Election Tracker</h1>
        <p>This application can only be opened with a valid <strong>single sign-on token</strong>
           issued by the dashboard.</p>
        <p class="muted">Opening this URL directly, or with an expired or reused token, will not grant access.</p>
        <a class="signin-btn" [href]="loginUrl">Sign in via SSO</a>
      </section>
    </main>
  `,
  styles: [`
    main { max-width: 1100px; margin: 64px auto; padding: 0 24px; }
    .card {
      background: #fff; border-radius: 10px; padding: 28px 32px; max-width: 560px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.06);
    }
    h1 { margin: 4px 0 10px; font-size: 24px; }
    p { color: #444; line-height: 1.55; }
    .muted { color: #5f6368; font-size: 13px; }
    .badge {
      display: inline-block; background: #fce8e6; color: #c5221f;
      padding: 3px 10px; border-radius: 99px; font-size: 12px; margin-bottom: 8px;
    }
    .signin-btn {
      display: inline-block; margin-top: 14px; background: #174a9b; color: #fff;
      text-decoration: none; padding: 10px 22px; border-radius: 6px; font-weight: 600;
      box-shadow: 0 2px 6px rgba(0,0,0,0.08); transition: filter .15s, transform .1s;
    }
    .signin-btn:hover { filter: brightness(1.12); transform: translateY(-1px); }
  `],
})
export class DeniedComponent {
  // Send the user to the central login, asking to be returned to this app afterwards.
  loginUrl = `${environment.loginUrl}?returnUrl=${encodeURIComponent(window.location.origin + '/')}`;
}
