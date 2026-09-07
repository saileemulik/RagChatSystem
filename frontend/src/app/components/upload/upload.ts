import { Component, signal, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IngestResponse } from '../../models/query.model';
import { Injestion } from '../../injestion';
import { UploadStateService } from '../../services/upload-state.service';
import { FileVerificationComponent } from '../file-verification/file-verification';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [CommonModule, FileVerificationComponent],
  templateUrl: './upload.html',
  styleUrl: './upload.css',
})
export class Upload {
  @Output() fileUploaded = new EventEmitter<IngestResponse>();
  
  uploadStatus = signal<{ message: string; type: string } | null>(null);
  selectedFile = signal<File | null>(null);

  constructor(
    private ingestService: Injestion,
    public uploadStateService: UploadStateService
  ) {}

  onFileSelect(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile.set(file);
      this.uploadStatus.set(null);
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.selectedFile.set(files[0]);
      this.uploadStatus.set(null);
    }
  }

  onUploadComplete(response: IngestResponse) {
    this.uploadStatus.set({ 
      message: `Successfully uploaded ${response.fileName} (${response.chunksCreated} chunks created)`, 
      type: 'success' 
    });
    this.fileUploaded.emit(response);
  }

  onUploadError(error: string) {
    this.uploadStatus.set({ 
      message: `Upload failed: ${error}`, 
      type: 'error' 
    });
  }

  clearFile() {
    this.selectedFile.set(null);
    this.uploadStatus.set(null);
  }
}
