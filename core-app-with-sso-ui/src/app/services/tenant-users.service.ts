import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateTenantUserDto,
  ResetPasswordDto,
  TenantUserDto,
  UpdateTenantUserDto
} from '../models/app.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TenantUsersService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/tenant/users`;

  constructor(private http: HttpClient) {}

  list(): Observable<TenantUserDto[]> {
    return this.http.get<TenantUserDto[]>(this.apiUrl);
  }

  create(dto: CreateTenantUserDto): Observable<TenantUserDto> {
    return this.http.post<TenantUserDto>(this.apiUrl, dto);
  }

  update(userId: number, dto: UpdateTenantUserDto): Observable<TenantUserDto> {
    return this.http.put<TenantUserDto>(`${this.apiUrl}/${userId}`, dto);
  }

  remove(userId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${userId}`);
  }

  resetPassword(userId: number, dto: ResetPasswordDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${userId}/reset-password`, dto);
  }
}
