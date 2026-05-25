import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateTenantDto,
  SeedTenantUserDto,
  TenantDto,
  TenantUserDto,
  UpdateTenantDto
} from '../models/app.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TenantAdminService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/admin/tenants`;

  constructor(private http: HttpClient) {}

  list(): Observable<TenantDto[]> {
    return this.http.get<TenantDto[]>(this.apiUrl);
  }

  get(id: number): Observable<TenantDto> {
    return this.http.get<TenantDto>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateTenantDto): Observable<TenantDto> {
    return this.http.post<TenantDto>(this.apiUrl, dto);
  }

  update(id: number, dto: UpdateTenantDto): Observable<TenantDto> {
    return this.http.put<TenantDto>(`${this.apiUrl}/${id}`, dto);
  }

  deactivate(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  seedUser(tenantId: number, dto: SeedTenantUserDto): Observable<TenantUserDto> {
    return this.http.post<TenantUserDto>(`${this.apiUrl}/${tenantId}/users`, dto);
  }
}
