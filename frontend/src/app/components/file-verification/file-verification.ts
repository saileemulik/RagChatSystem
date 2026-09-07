import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Injestion } from '../../injestion';
import { IngestResponse } from '../../models/query.model';

@Component({
  selector: 'app-file-verification',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './file-verification.html',
  styleUrl: './file-verification.css'
})
export class FileVerificationComponent implements OnInit {
  @Input() file!: File;
  @Output() uploadComplete = new EventEmitter<IngestResponse>();
  @Output() uploadError = new EventEmitter<string>();
  
  isUploading = false;
  uploadResult: IngestResponse | null = null;
  
  constructor(private injestionService: Injestion) {}
  
  get fileIcon(): string {
    const ext = this.file.name.split('.').pop()?.toLowerCase();
    switch (ext) {
      case 'pdf': return '📄';
      case 'docx':
      case 'doc': return '📝';
      case 'xlsx':
      case 'xls': return '📊';
      case 'txt': return '📃';
      case 'csv': return '📋';
      default: return '📁';
    }
  }

  get isValidFile(): boolean {
    const validExtensions = ['pdf', 'docx', 'doc', 'xlsx', 'xls', 'txt', 'csv'];
    const ext = this.file.name.split('.').pop()?.toLowerCase();
    return validExtensions.includes(ext || '');
  }

  get fileSizeFormatted(): string {
    const bytes = this.file.size;
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  get validationMessage(): string {
    if (!this.isValidFile) {
      return 'Unsupported file type. Please upload PDF, DOCX, XLSX, TXT, or CSV files.';
    }
    if (this.file.size > 10 * 1024 * 1024) { // 10MB limit
      return 'File size exceeds 10MB limit.';
    }
    return 'File is ready for upload.';
  }

  get isValid(): boolean {
    return this.isValidFile && this.file.size <= 10 * 1024 * 1024;
  }

  ngOnInit() {
    if (this.isValid) {
      this.uploadFile();
    }
  }

  private uploadFile() {
    this.isUploading = true;
    this.injestionService.uploadDocument(this.file).subscribe({
      next: (response) => {
        this.uploadResult = response;
        this.isUploading = false;
        this.uploadComplete.emit(response);
      },
      error: (error) => {
        this.isUploading = false;
        let errorMessage = 'Upload failed. Please try again.';
        
        if (error.error?.message) {
          errorMessage = error.error.message;
        } else if (error.message) {
          errorMessage = error.message;
        } else if (typeof error === 'string') {
          errorMessage = error;
        }
        
        this.uploadError.emit(errorMessage);
      }
    });
  }
}