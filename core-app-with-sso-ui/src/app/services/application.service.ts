import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApplicationDto, SSORedirect } from '../models/app.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApplicationService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api`;

  constructor(private http: HttpClient) {}

  getMyApplications(): Observable<ApplicationDto[]> {
    return this.http.get<ApplicationDto[]>(`${this.apiUrl}/applications/my-apps`);
  }

  generateSSOToken(applicationId: number): Observable<SSORedirect> {
    return this.http.post<SSORedirect>(`${this.apiUrl}/sso/generate/${applicationId}`, {});
  }
}
