import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, Subscription, of, tap, timer, switchMap, catchError } from 'rxjs';
import {
  AuthResponse,
  LoginDto,
  LoginResponse,
  SelectTenantDto,
  SessionStatus,
  TenantSummary
} from '../models/app.models';
import { environment } from '../../environments/environment';

interface StoredSession {
  token: string;
  fullName: string;
  email: string;
  tenantId?: number;
  tenantName?: string;
  tenantLogoUrl?: string | null;
  role: string;
  isSuperAdmin: boolean;
  sessionId?: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/auth`;
  private readonly ssoApiUrl = `${environment.apiBaseUrl}/api/sso`;
  private readonly tokenKey = 'dashboard_token';
  private readonly userKey = 'dashboard_user';
  private readonly interimKey = 'dashboard_interim_token';
  private readonly tenantsKey = 'dashboard_pending_tenants';
  private readonly pollMs = 10_000;

  // ── Reactive state ──
  // These signals are the single source of truth. localStorage is kept in sync
  // for persistence across reloads, but the UI reads from the signals — never
  // from localStorage directly — so change detection always fires.
  private _isLoggedIn = signal(this.hasToken());
  private _currentUser = signal<StoredSession | null>(this.loadFromStorage());

  isLoggedIn = this._isLoggedIn.asReadonly();
  currentUser = this._currentUser.asReadonly();
  isSuperAdmin = computed(() => this._currentUser()?.isSuperAdmin === true);

  private poll?: Subscription;

  constructor(private http: HttpClient, private router: Router) {
    if (this._currentUser()?.sessionId) this.startPolling();
  }

  login(dto: LoginDto): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, dto).pipe(
      tap(res => {
        if (res.isSuperAdmin) {
          this.storeSuperAdmin(res);
        } else {
          this.storeInterim(res);
        }
      })
    );
  }

  selectTenant(dto: SelectTenantDto): Observable<AuthResponse> {
    const interim = this.getInterimToken();
    const headers = new HttpHeaders(interim ? { Authorization: `Bearer ${interim}` } : {});
    return this.http.post<AuthResponse>(`${this.apiUrl}/select-tenant`, dto, { headers }).pipe(
      tap(res => this.storeTenantAuth(res))
    );
  }

  logout(): void {
    const session = this._currentUser();
    const finish = () => this.clearAndRedirect();

    if (session?.sessionId) {
      this.http.post(`${this.ssoApiUrl}/logout`, { sessionId: session.sessionId })
        .pipe(catchError(() => of(null)))
        .subscribe(() => finish());
    } else {
      finish();
    }
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getInterimToken(): string | null {
    return localStorage.getItem(this.interimKey);
  }

  getPendingTenants(): TenantSummary[] {
    const stored = localStorage.getItem(this.tenantsKey);
    return stored ? JSON.parse(stored) as TenantSummary[] : [];
  }

  // ── Private ──

  private hasToken(): boolean {
    return !!localStorage.getItem(this.tokenKey);
  }

  private loadFromStorage(): StoredSession | null {
    try {
      const raw = localStorage.getItem(this.userKey);
      return raw ? JSON.parse(raw) as StoredSession : null;
    } catch {
      return null;
    }
  }

  private clearAndRedirect(): void {
    this.stopPolling();
    this._currentUser.set(null);
    this._isLoggedIn.set(false);
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    localStorage.removeItem(this.interimKey);
    localStorage.removeItem(this.tenantsKey);
    this.router.navigate(['/login']);
  }

  private storeTenantAuth(res: AuthResponse): void {
    const session: StoredSession = {
      token: res.token,
      fullName: res.fullName,
      email: res.email,
      tenantId: res.tenantId,
      tenantName: res.tenantName,
      tenantLogoUrl: res.tenantLogoUrl,
      role: res.role,
      isSuperAdmin: false,
      sessionId: res.sessionId
    };
    localStorage.setItem(this.tokenKey, res.token);
    localStorage.setItem(this.userKey, JSON.stringify(session));
    localStorage.removeItem(this.interimKey);
    localStorage.removeItem(this.tenantsKey);
    this._currentUser.set(session);
    this._isLoggedIn.set(true);
    this.startPolling();
  }

  private storeSuperAdmin(res: LoginResponse): void {
    const session: StoredSession = {
      token: res.token,
      fullName: res.fullName,
      email: res.email,
      role: 'SuperAdmin',
      isSuperAdmin: true
    };
    localStorage.setItem(this.tokenKey, res.token);
    localStorage.setItem(this.userKey, JSON.stringify(session));
    localStorage.removeItem(this.interimKey);
    localStorage.removeItem(this.tenantsKey);
    this._currentUser.set(session);
    this._isLoggedIn.set(true);
  }

  private storeInterim(res: LoginResponse): void {
    localStorage.setItem(this.interimKey, res.interimToken);
    localStorage.setItem(this.tenantsKey, JSON.stringify(res.tenants));
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this._currentUser.set(null);
    this._isLoggedIn.set(false);
  }

  private startPolling(): void {
    this.stopPolling();
    this.poll = timer(this.pollMs, this.pollMs).pipe(
      switchMap(() => {
        const sessionId = this._currentUser()?.sessionId;
        if (!sessionId) return of(null);
        return this.http.get<SessionStatus>(`${this.ssoApiUrl}/sessions/${sessionId}`)
          .pipe(catchError(() => of(null)));
      })
    ).subscribe(res => {
      if (res && res.active === false) {
        this.clearAndRedirect();
      }
    });
  }

  private stopPolling(): void {
    this.poll?.unsubscribe();
    this.poll = undefined;
  }
}
