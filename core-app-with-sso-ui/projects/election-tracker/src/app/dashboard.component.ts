import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { EMPTY, Subscription, catchError, interval, switchMap } from 'rxjs';
import { ElectionService, ElectionData, SessionInfo } from './election.service';
import { environment } from '../environments/environment';

@Component({
  selector: 'et-dashboard',
  template: `
    <header class="app-header">
      <h2>Election Tracker</h2>
      @if (session(); as s) {
        <button class="logout-btn" (click)="logout()" title="Sign out of every app">Log out</button>
      }
    </header>

    <main>
      @if (session(); as s) {
        <div class="dashboard-head">
          <div>
            <h1>Live Results</h1>
            <p class="muted">Welcome back, <strong>{{ s.fullName }}</strong> —
              signed in as <span class="pill">{{ s.role }}</span></p>
          </div>
          <div class="meta">
            <div><span class="muted small">User</span> {{ s.email }}</div>
            <div><span class="muted small">Tenant</span> #{{ s.tenantId }}</div>
          </div>
        </div>
      }

      @if (data(); as d) {
        <div class="tile-grid">
          @for (t of d.tiles; track t.title) {
            <div class="tile">
              <div class="tile-icon">{{ t.icon }}</div>
              <div class="tile-body">
                <div class="tile-title">{{ t.title }}</div>
                <div class="tile-subtitle">{{ t.subtitle }}</div>
              </div>
              <div class="tile-metric">{{ t.metric }}</div>
            </div>
          }
        </div>
        <p class="muted small foot">
          Data served only to a validated SSO session · generated {{ d.generatedAtUtc }} UTC
        </p>
      } @else {
        <p class="muted">Loading election data…</p>
      }
    </main>
  `,
  styles: [`
    .app-header {
      background: #174a9b; color: #fff; padding: 14px 32px;
      display: flex; align-items: center; justify-content: space-between;
    }
    .app-header h2 { margin: 0; font-size: 18px; letter-spacing: .4px; }
    .logout-btn {
      background: rgba(0,0,0,.18); color: #fff; border: 1px solid rgba(255,255,255,.35);
      padding: 6px 14px; border-radius: 999px; font-size: 13px; cursor: pointer;
    }
    .logout-btn:hover { background: rgba(0,0,0,.32); }

    main { max-width: 1100px; margin: 32px auto; padding: 0 24px; }
    .dashboard-head {
      display: flex; justify-content: space-between; align-items: flex-start; gap: 24px;
      background: #fff; padding: 24px 28px; border-radius: 10px;
      box-shadow: 0 2px 8px rgba(0,0,0,.06); margin-bottom: 24px;
    }
    .dashboard-head h1 { margin: 0 0 6px; font-size: 22px; }
    .meta { text-align: right; font-size: 13px; line-height: 1.7; }

    .tile-grid {
      display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 16px;
    }
    .tile {
      background: #fff; border-radius: 10px; padding: 18px 20px;
      box-shadow: 0 2px 6px rgba(0,0,0,.05);
      display: grid; grid-template-columns: 44px 1fr auto; align-items: center; gap: 14px;
      border-left: 4px solid #174a9b; transition: transform .15s, box-shadow .15s;
    }
    .tile:hover { transform: translateY(-1px); box-shadow: 0 4px 12px rgba(0,0,0,.08); }
    .tile-icon { font-size: 26px; text-align: center; }
    .tile-title { font-size: 15px; font-weight: 600; }
    .tile-subtitle { font-size: 12px; color: #5f6368; margin-top: 2px; }
    .tile-metric { font-size: 22px; font-weight: 700; color: #174a9b; }

    .muted { color: #5f6368; }
    .small { font-size: 12px; }
    .pill { background: #e8f0fe; color: #1a73e8; padding: 2px 10px; border-radius: 10px; font-size: 12px; }
    .foot { margin-top: 24px; text-align: center; }
  `],
})
export class DashboardComponent implements OnInit, OnDestroy {
  private svc = inject(ElectionService);
  private poll?: Subscription;

  session = signal<SessionInfo | null>(null);
  data = signal<ElectionData | null>(null);

  ngOnInit(): void {
    this.svc.getSession().subscribe({ next: (s) => this.session.set(s) });
    this.svc.getData().subscribe({ next: (d) => this.data.set(d) });

    // Cross-app logout detection: if the SSO session is revoked anywhere (e.g. the user
    // signs out of the dashboard or another app), /session starts returning 401 — bounce to login.
    this.poll = interval(7000)
      .pipe(
        switchMap(() =>
          this.svc.getSession().pipe(
            catchError(() => {
              this.redirectToLogin();
              return EMPTY;
            }),
          ),
        ),
      )
      .subscribe((s) => this.session.set(s));
  }

  ngOnDestroy(): void {
    this.poll?.unsubscribe();
  }

  logout(): void {
    this.poll?.unsubscribe();
    this.svc.logout().subscribe({
      next: (r) => (window.location.href = r.loginUrl),
      error: () => this.redirectToLogin(),
    });
  }

  private redirectToLogin(): void {
    const returnUrl = encodeURIComponent(window.location.origin + '/');
    window.location.href = `${environment.loginUrl}?returnUrl=${returnUrl}`;
  }
}
