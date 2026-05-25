import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AdminLookupResponse, AdminTemplateRow } from '../models/app.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AdminTemplatesService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/admin`;

  constructor(private http: HttpClient) {}

  /** Flat list of every template across every tenant. */
  listAll(): Observable<AdminTemplateRow[]> {
    return this.http.get<AdminTemplateRow[]>(`${this.apiUrl}/templates`);
  }

  /** Look up tenant + template by clientCode + templateName (both case-insensitive). */
  lookup(clientCode: string, templateName: string): Observable<AdminLookupResponse> {
    const params = new HttpParams()
      .set('clientCode', clientCode)
      .set('templateName', templateName);
    return this.http.get<AdminLookupResponse>(`${this.apiUrl}/lookup`, { params });
  }
}
