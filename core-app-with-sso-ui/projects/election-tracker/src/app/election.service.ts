import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

export interface SessionInfo {
  userId: number;
  tenantId: number;
  email: string;
  fullName: string;
  role: string;
}

export interface ElectionTile {
  title: string;
  subtitle: string;
  metric: string;
  icon: string;
}

export interface ElectionData {
  generatedAtUtc: string;
  tiles: ElectionTile[];
}

@Injectable({ providedIn: 'root' })
export class ElectionService {
  private http = inject(HttpClient);
  private readonly api = `${environment.apiBaseUrl}/api/election`;

  /** Exchange a one-time SSO token for a server-side session (sets the HttpOnly cookie). */
  validateToken(token: string): Observable<SessionInfo> {
    return this.http.post<SessionInfo>(`${this.api}/sso/callback`, { token });
  }

  /** Current signed-in user; errors (401) when there is no valid session. */
  getSession(): Observable<SessionInfo> {
    return this.http.get<SessionInfo>(`${this.api}/session`);
  }

  /** Election dashboard data; only succeeds with a valid session. */
  getData(): Observable<ElectionData> {
    return this.http.get<ElectionData>(`${this.api}/data`);
  }

  /** Global sign-out; returns the central login URL to redirect to. */
  logout(): Observable<{ loginUrl: string }> {
    return this.http.post<{ loginUrl: string }>(`${this.api}/logout`, {});
  }
}
