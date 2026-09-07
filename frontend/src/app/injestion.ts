import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { IngestResponse, DocumentInfo } from './models/query.model';

@Injectable({
  providedIn: 'root',
})
export class Injestion {
  private apiUrl = 'http://localhost:5149/api';

  constructor(private http: HttpClient) {}

  uploadDocument(file: File, metadata?: string): Observable<IngestResponse> {
    const formData = new FormData();
    formData.append('file', file);
    if (metadata) {
      formData.append('metadata', metadata);
    }

    return this.http.post<IngestResponse>(`${this.apiUrl}/Ingest`, formData);
  }

  getDocuments(): Observable<DocumentInfo[]> {
    return this.http.get<{documents: DocumentInfo[], totalCount: number}>(`${this.apiUrl}/Documents`)
      .pipe(
        map(response => response.documents)
      );
  }

  getDocumentContent(documentId: string): Observable<{content: string}> {
    return this.http.get<{content: string}>(`${this.apiUrl}/Ingest/${documentId}/content`);
  }

  deleteDocument(documentId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/Documents/${documentId}`);
  }
}
