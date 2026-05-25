import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateJsonTemplateDto,
  JsonTemplateDto,
  JsonTemplateSummary,
  UpdateJsonTemplateDto
} from '../models/app.models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TemplatesService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/tenant/templates`;

  constructor(private http: HttpClient) {}

  list(): Observable<JsonTemplateSummary[]> {
    return this.http.get<JsonTemplateSummary[]>(this.apiUrl);
  }

  get(id: number): Observable<JsonTemplateDto> {
    return this.http.get<JsonTemplateDto>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateJsonTemplateDto): Observable<JsonTemplateDto> {
    return this.http.post<JsonTemplateDto>(this.apiUrl, dto);
  }

  update(id: number, dto: UpdateJsonTemplateDto): Observable<JsonTemplateDto> {
    return this.http.put<JsonTemplateDto>(`${this.apiUrl}/${id}`, dto);
  }

  remove(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
