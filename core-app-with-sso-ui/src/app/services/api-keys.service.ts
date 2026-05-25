import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiKeySummary, CreateApiKeyDto, CreateApiKeyResponse } from '../models/app.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiKeysService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/tenant/api-keys`;

  constructor(private http: HttpClient) {}

  list(): Observable<ApiKeySummary[]> {
    return this.http.get<ApiKeySummary[]>(this.apiUrl);
  }

  create(dto: CreateApiKeyDto): Observable<CreateApiKeyResponse> {
    return this.http.post<CreateApiKeyResponse>(this.apiUrl, dto);
  }

  revoke(id: number): Observable<ApiKeySummary> {
    return this.http.post<ApiKeySummary>(`${this.apiUrl}/${id}/revoke`, {});
  }

  remove(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
