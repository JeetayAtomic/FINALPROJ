import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ApplicationAdminDto,
  CreateApplicationDto,
  TenantApplicationDto,
  TenantApplicationWriteDto,
  UpdateApplicationDto
} from '../models/app.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApplicationsAdminService {
  private readonly base = `${environment.apiBaseUrl}/api/admin`;

  constructor(private http: HttpClient) {}

  // Catalog CRUD
  list(): Observable<ApplicationAdminDto[]> {
    return this.http.get<ApplicationAdminDto[]>(`${this.base}/applications`);
  }
  get(id: number): Observable<ApplicationAdminDto> {
    return this.http.get<ApplicationAdminDto>(`${this.base}/applications/${id}`);
  }
  create(dto: CreateApplicationDto): Observable<ApplicationAdminDto> {
    return this.http.post<ApplicationAdminDto>(`${this.base}/applications`, dto);
  }
  update(id: number, dto: UpdateApplicationDto): Observable<ApplicationAdminDto> {
    return this.http.put<ApplicationAdminDto>(`${this.base}/applications/${id}`, dto);
  }
  deactivate(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/applications/${id}`);
  }

  // Tenant subscriptions
  listTenantApps(tenantId: number): Observable<TenantApplicationDto[]> {
    return this.http.get<TenantApplicationDto[]>(`${this.base}/tenants/${tenantId}/applications`);
  }
  upsertTenantApp(tenantId: number, applicationId: number, dto: TenantApplicationWriteDto): Observable<TenantApplicationDto> {
    return this.http.put<TenantApplicationDto>(`${this.base}/tenants/${tenantId}/applications/${applicationId}`, dto);
  }
  removeTenantApp(tenantId: number, applicationId: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/tenants/${tenantId}/applications/${applicationId}`);
  }
}
