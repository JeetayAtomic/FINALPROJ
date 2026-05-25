import { Component, Inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { APP_CONFIG, SampleAppConfig } from './app-config';
import { SessionService } from './session.service';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="shell">
      <div class="panel">
        <header [style.background]="config.color" class="mini">
          <strong>{{ config.name }}</strong>
        </header>
        <div class="body">
          <h1>No active session</h1>
          <p class="muted">
            This is the <strong>{{ config.name }}</strong> sample application.
            You need to launch it from the Dashboard so it receives a valid SSO token.
          </p>
          <p *ngIf="notice()" class="notice">{{ notice() }}</p>
          <a class="btn" href="http://localhost:4200" target="_self">Open Dashboard</a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .shell { min-height: 100vh; display: flex; align-items: center; justify-content: center; background: #f5f6fa; padding: 16px; }
    .panel { background: white; border-radius: 10px; max-width: 460px; width: 100%; box-shadow: 0 2px 8px rgba(0,0,0,0.06); overflow: hidden; }
    .mini { color: white; padding: 12px 20px; }
    .body { padding: 24px 28px 28px; }
    h1 { margin: 6px 0; font-size: 22px; color: #202124; }
    .muted { color: #5f6368; line-height: 1.5; }
    .notice { background: #fff8e1; border-left: 4px solid #fbbc05; padding: 10px 14px; border-radius: 4px; color: #52410d; font-size: 14px; }
    .btn { display: inline-block; margin-top: 12px; background: #4285F4; color: white; padding: 10px 18px; border-radius: 6px; text-decoration: none; font-size: 14px; }
    .btn:hover { background: #3367d6; }
  `]
})
export class LandingComponent implements OnInit {
  notice = signal<string | null>(null);
  constructor(
    @Inject(APP_CONFIG) public config: SampleAppConfig,
    private session: SessionService
  ) {}
  ngOnInit(): void {
    this.notice.set(this.session.readNoticeOnce());
  }
}
