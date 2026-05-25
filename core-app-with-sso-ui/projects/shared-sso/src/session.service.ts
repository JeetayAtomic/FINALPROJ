import { Injectable, Inject, OnDestroy, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, Subscription, timer, of } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { APP_CONFIG, SampleAppConfig } from './app-config';
import { SessionStatus, SsoValidateRequest, SsoValidateResponse, StoredSession } from './models';

/**
 * Holds the target-app session derived from an SSO token.
 * - Stores it in localStorage so reloads keep the user signed in until the session
 *   is revoked on the server.
 * - Polls the server every `pollMs` ms; if the server returns `active=false` the
 *   local session is cleared and the app redirects to the landing page. This is
 *   how cross-app logout propagates.
 */
@Injectable({ providedIn: 'root' })
export class SessionService implements OnDestroy {
  private readonly pollMs = 10_000;

  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly config: SampleAppConfig;
  private poll?: Subscription;

  private readonly _session = signal<StoredSession | null>(null);
  readonly session = this._session.asReadonly();
  readonly isSignedIn = computed(() => this._session() != null);

  constructor(@Inject(APP_CONFIG) config: SampleAppConfig) {
    this.config = config;
    const raw = localStorage.getItem(this.storageKey);
    if (raw) {
      try {
        this._session.set(JSON.parse(raw) as StoredSession);
      } catch {
        localStorage.removeItem(this.storageKey);
      }
    }
    if (this._session()) this.startPolling();
  }

  ngOnDestroy(): void {
    this.poll?.unsubscribe();
  }

  /** Step 1 of bootstrap: exchange the one-time SSO token for a server-side session. */
  validate(token: string): Observable<SsoValidateResponse> {
    const req: SsoValidateRequest = { token };
    return this.http.post<SsoValidateResponse>(`${this.config.apiBaseUrl}/api/sso/validate`, req).pipe(
      tap(res => {
        if (res.isValid) {
          const stored: StoredSession = {
            sessionId: res.sessionId,
            userId: res.userId,
            tenantId: res.tenantId,
            applicationId: res.applicationId,
            email: res.email,
            fullName: res.fullName,
            role: res.role,
            signedInAt: new Date().toISOString()
          };
          this._session.set(stored);
          localStorage.setItem(this.storageKey, JSON.stringify(stored));
          this.startPolling();
        }
      })
    );
  }

  /** Signs the user out everywhere. Server revokes all sessions for (UserId, TenantId). */
  logout(): void {
    const s = this._session();
    if (!s) {
      this.router.navigate(['/']);
      return;
    }
    this.http.post(`${this.config.apiBaseUrl}/api/sso/logout`, { sessionId: s.sessionId })
      .pipe(catchError(() => of(null)))
      .subscribe(() => this.clearLocalAndGoHome('You have been signed out.'));
  }

  /** Called when the server tells us the session is revoked. */
  private clearLocalAndGoHome(reason?: string): void {
    this.poll?.unsubscribe();
    this.poll = undefined;
    this._session.set(null);
    localStorage.removeItem(this.storageKey);
    if (reason) {
      sessionStorage.setItem(this.noticeKey, reason);
    }
    this.router.navigate(['/']);
  }

  private startPolling(): void {
    this.poll?.unsubscribe();
    this.poll = timer(this.pollMs, this.pollMs).pipe(
      switchMap(() => {
        const s = this._session();
        if (!s) return of(null);
        return this.http.get<SessionStatus>(`${this.config.apiBaseUrl}/api/sso/sessions/${s.sessionId}`)
          .pipe(catchError(() => of(null)));
      })
    ).subscribe(res => {
      if (res && res.active === false) {
        this.clearLocalAndGoHome('You were signed out from another app.');
      }
    });
  }

  readNoticeOnce(): string | null {
    const v = sessionStorage.getItem(this.noticeKey);
    if (v) sessionStorage.removeItem(this.noticeKey);
    return v;
  }

  private get storageKey() { return `${this.config.storageKey}_session`; }
  private get noticeKey()  { return `${this.config.storageKey}_notice`; }
}
