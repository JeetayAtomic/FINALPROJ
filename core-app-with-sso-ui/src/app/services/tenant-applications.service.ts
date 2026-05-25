import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TenantApplicationDto, TenantApplicationWriteDto } from '../models/app.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TenantApplicationsService {
  private readonly base = `${environment.apiBaseUrl}/api/tenant/applications`;

  constructor(private http: HttpClient) {}

  list(): Observable<TenantApplicationDto[]> {
    return this.http.get<TenantApplicationDto[]>(this.base);
  }

  toggle(applicationId: number, dto: TenantApplicationWriteDto): Observable<TenantApplicationDto> {
    return this.http.put<TenantApplicationDto>(`${this.base}/${applicationId}`, dto);
  }
}
