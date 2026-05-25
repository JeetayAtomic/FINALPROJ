import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';


@Injectable({
  providedIn: 'root'
})
export class SearchService { 
  

  constructor(private http: HttpClient) {}

  searchClients(query: string, profile: string): Observable<any> {
    let params = new HttpParams();
    if (query) {
      params = params.set('q', query);
    }
    if (profile) {
      params = params.set('profile', profile);
    }
    
     return this.http.get<any>(
       `${environment.apiBaseUrl}/api/ClientAPI/GetClients`,
    { params }
  );
  }
}
