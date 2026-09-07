import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { QueryRequest, QueryResponse, AgenticQueryRequest, AgenticQueryResponse } from './models/query.model';

@Injectable({
  providedIn: 'root',
})
export class Rag {
  private apiUrl = 'http://localhost:5149/api';

  constructor(private http: HttpClient) {}

  query(request: QueryRequest): Observable<QueryResponse> {
    return this.http.post<QueryResponse>(`${this.apiUrl}/Query`, request);
  }

  agenticQuery(request: AgenticQueryRequest): Observable<AgenticQueryResponse> {
    return this.http.post<AgenticQueryResponse>(`${this.apiUrl}/AgenticQuery`, request);
  }
}
