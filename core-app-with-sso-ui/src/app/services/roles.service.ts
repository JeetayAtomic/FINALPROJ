import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ApplicationDto,
  CreateRoleDto,
  RoleDto,
  UpdateRoleDto
} from '../models/app.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class RolesService {
  private readonly rolesUrl = `${environment.apiBaseUrl}/api/tenant/roles`;
  private readonly tenantCatalogUrl = `${environment.apiBaseUrl}/api/Applications/tenant-catalog`;

  constructor(private http: HttpClient) {}

  list(): Observable<RoleDto[]> {
    return this.http.get<RoleDto[]>(this.rolesUrl);
  }

  get(id: number): Observable<RoleDto> {
    return this.http.get<RoleDto>(`${this.rolesUrl}/${id}`);
  }

  create(dto: CreateRoleDto): Observable<RoleDto> {
    return this.http.post<RoleDto>(this.rolesUrl, dto);
  }

  update(id: number, dto: UpdateRoleDto): Observable<RoleDto> {
    return this.http.put<RoleDto>(`${this.rolesUrl}/${id}`, dto);
  }

  remove(id: number): Observable<void> {
    return this.http.delete<void>(`${this.rolesUrl}/${id}`);
  }

  /** Apps the current tenant is subscribed to — the pool a role can grant from. */
  tenantCatalog(): Observable<ApplicationDto[]> {
    return this.http.get<ApplicationDto[]>(this.tenantCatalogUrl);
  }
}
