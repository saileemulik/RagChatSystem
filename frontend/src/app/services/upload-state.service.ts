import { Injectable, signal } from '@angular/core';
import { IngestResponse } from '../models/query.model';

@Injectable({
  providedIn: 'root'
})
export class UploadStateService {
  private _uploadedFiles = signal<IngestResponse[]>([]);

  get uploadedFiles() {
    return this._uploadedFiles.asReadonly();
  }

  addUploadedFile(file: IngestResponse) {
    this._uploadedFiles.update(files => [...files, file]);
  }

  hasUploadedFiles(): boolean {
    return this._uploadedFiles().length > 0;
  }
}