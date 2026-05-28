import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom, Observable } from 'rxjs';
import { ApplicationDto, SSORedirect } from '../models/app.models';
import { environment } from '../../environments/environment';

const RETURN_URL_STORAGE_KEY = 'sso_return_url';

@Injectable({ providedIn: 'root' })
export class ApplicationService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api`;

  constructor(private http: HttpClient, private router: Router) {}

  getMyApplications(): Observable<ApplicationDto[]> {
    return this.http.get<ApplicationDto[]>(`${this.apiUrl}/applications/my-apps`);
  }

  generateSSOToken(applicationId: number): Observable<SSORedirect> {
    return this.http.post<SSORedirect>(`${this.apiUrl}/sso/generate/${applicationId}`, {});
  }

  /**
   * Capture a returnUrl handed to us by a sample app's logout redirect, so it
   * survives the login → optional tenant-select hop. Safe to call with null.
   */
  captureReturnUrl(returnUrl: string | null | undefined): void {
    if (returnUrl) sessionStorage.setItem(RETURN_URL_STORAGE_KEY, returnUrl);
  }

  /**
   * Navigate after a successful sign-in. If a returnUrl was captured at login
   * time and matches one of the user's apps, mint a fresh SSO token and bounce
   * the browser there. Otherwise fall back to the dashboard (or admin home).
   */
  async redirectAfterLogin(isSuperAdmin: boolean): Promise<void> {
    const returnUrl = sessionStorage.getItem(RETURN_URL_STORAGE_KEY);
    sessionStorage.removeItem(RETURN_URL_STORAGE_KEY);

    if (isSuperAdmin || !returnUrl) {
      this.router.navigate([isSuperAdmin ? '/admin/tenants' : '/dashboard']);
      return;
    }

    try {
      const apps = await firstValueFrom(this.getMyApplications());
      const match = apps.find(a => this.urlMatchesApp(returnUrl, a.baseUrl));
      if (!match) {
        this.router.navigate(['/dashboard']);
        return;
      }
      const sso = await firstValueFrom(this.generateSSOToken(match.id));
      window.location.href = sso.redirectUrl;
    } catch {
      this.router.navigate(['/dashboard']);
    }
  }

  // Only accept returnUrls whose origin+path-prefix match a known app baseUrl —
  // never trust an arbitrary value from a query param (open-redirect guard).
  private urlMatchesApp(returnUrl: string, baseUrl: string): boolean {
    try {
      const ret = new URL(returnUrl);
      const base = new URL(baseUrl);
      return ret.origin === base.origin && ret.pathname.startsWith(base.pathname);
    } catch {
      return false;
    }
  }
}
