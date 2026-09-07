import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatHistoryService } from '../../services/chat-history.service';

@Component({
  selector: 'app-chat-sidebar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './chat-sidebar.html',
  styleUrl: './chat-sidebar.css'
})
export class ChatSidebarComponent {
  @Output() backClicked = new EventEmitter<void>();
  
  constructor(public chatHistoryService: ChatHistoryService) {}

  goHome() {
    this.backClicked.emit();
  }

  newChat() {
    this.chatHistoryService.createNewConversation();
  }

  selectConversation(id: string) {
    this.chatHistoryService.selectConversation(id);
  }

  deleteConversation(id: string, event: Event) {
    event.stopPropagation();
    if (confirm('Delete this conversation?')) {
      this.chatHistoryService.deleteConversation(id);
    }
  }

  clearAll() {
    if (confirm('Delete all conversations?')) {
      this.chatHistoryService.clearAllHistory();
    }
  }
}