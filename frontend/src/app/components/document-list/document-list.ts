import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Injestion } from '../../injestion';
import { DocumentInfo } from '../../models/query.model';

@Component({
  selector: 'app-document-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './document-list.html',
  styleUrl: './document-list.css'
})
export class DocumentListComponent implements OnInit {
  documents = signal<DocumentInfo[]>([]);
  isLoading = signal(false);
  expandedDoc = signal<string | null>(null);
  loadingContent = signal<string | null>(null);

  constructor(private injestionService: Injestion) {}

  ngOnInit() {
    this.loadDocuments();
  }

  loadDocuments() {
    this.isLoading.set(true);
    this.injestionService.getDocuments().subscribe({
      next: (docs) => {
        this.documents.set(docs);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Failed to load documents:', error);
        this.isLoading.set(false);
      }
    });
  }

  deleteDocument(documentId: string) {
    if (confirm('Are you sure you want to delete this document?')) {
      this.injestionService.deleteDocument(documentId).subscribe({
        next: () => {
          this.documents.update(docs => docs.filter(d => d.documentId !== documentId));
        },
        error: (error) => {
          console.error('Failed to delete document:', error);
        }
      });
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  toggleDocument(doc: DocumentInfo) {
    if (this.expandedDoc() === doc.documentId) {
      this.expandedDoc.set(null);
    } else {
      this.expandedDoc.set(doc.documentId);
      if (!doc.content) {
        this.loadDocumentContent(doc);
      }
    }
  }

  private loadDocumentContent(doc: DocumentInfo) {
    this.loadingContent.set(doc.documentId);
    this.injestionService.getDocumentContent(doc.documentId).subscribe({
      next: (response) => {
        const updatedDocs = this.documents().map(d => 
          d.documentId === doc.documentId ? {...d, content: response.content} : d
        );
        this.documents.set(updatedDocs);
        this.loadingContent.set(null);
      },
      error: (error) => {
        console.error('Failed to load document content:', error);
        this.loadingContent.set(null);
      }
    });
  }
}