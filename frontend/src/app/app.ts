import { Component, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Upload } from './components/upload/upload';
import { Chat } from './components/chat/chat';
import { DocumentListComponent } from './components/document-list/document-list';
import { ChatSidebarComponent } from './components/chat-sidebar/chat-sidebar';
import { UploadStateService } from './services/upload-state.service';
import { Injestion } from './injestion';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    Upload,
    Chat,
    DocumentListComponent,
    ChatSidebarComponent
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  @ViewChild('upload') upload!: Upload;
  
  currentView = signal('home');

  constructor(
    private uploadStateService: UploadStateService,
    private injestionService: Injestion
  ) {}

  onFileUploaded(file: any) {
    this.uploadStateService.addUploadedFile(file);
  }

  get hasUploadedFiles() {
    return this.uploadStateService.hasUploadedFiles();
  }

  goToChat() {
    this.injestionService.getDocuments().subscribe({
      next: (docs) => {
        if (docs.length === 0) {
          alert('Please upload a document first');
          this.goToUpload();
        } else {
          this.currentView.set('chat');
        }
      },
      error: () => {
        alert('Please upload a document first');
        this.goToUpload();
      }
    });
  }

  goToUpload() {
    this.currentView.set('upload');
  }

  goToDocuments() {
    this.currentView.set('documents');
  }

  goHome() {
    this.currentView.set('home');
  }
}
