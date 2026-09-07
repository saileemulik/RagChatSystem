import { Component, signal, OnInit, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Rag } from '../../rag';
import { QueryResponse, Citation, ToolUsage, AgenticQueryResponse } from '../../models/query.model';
import { ChatHistoryService } from '../../services/chat-history.service';
import { timeout, catchError, of } from 'rxjs';

@Component({
  selector: 'app-chat',
  standalone: true, 
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.html',
  styleUrl: './chat.css',
})
export class Chat implements OnInit {
  responses = signal<(QueryResponse | AgenticQueryResponse)[]>([]);
  currentCitations = signal<Citation[]>([]);
  currentToolsUsed = signal<ToolUsage[]>([]);
  isLoading = signal(false);
  loadingMessage = signal('Processing...');
  conversationId?: string;
  question = '';
  questions: string[] = [];
  useAgenticAI = signal(false); // Toggle for agentic AI

  constructor(
    private ragService: Rag,
    private chatHistoryService: ChatHistoryService
  ) {
    // Watch for conversation changes using effect
    effect(() => {
      this.chatHistoryService.activeConversationId();
      this.loadActiveConversation();
    });
  }

  ngOnInit() {
    this.loadActiveConversation();
  }

  loadActiveConversation() {
    const activeConv = this.chatHistoryService.activeConversation;
    if (activeConv && activeConv.messages.length > 0) {
      this.responses.set(activeConv.messages.map(msg => msg.response));
      this.questions = activeConv.messages.map(msg => msg.question);
      const lastResponse = activeConv.messages[activeConv.messages.length - 1]?.response;
      if (lastResponse) {
        this.conversationId = lastResponse.conversationId;
        this.currentCitations.set(lastResponse.citations);
        // Check if it's an agentic response
        if ('toolsUsed' in lastResponse) {
          this.currentToolsUsed.set((lastResponse as AgenticQueryResponse).toolsUsed);
        }
      }
    } else {
      this.responses.set([]);
      this.questions = [];
      this.conversationId = undefined;
      this.currentCitations.set([]);
      this.currentToolsUsed.set([]);
    }
  }

  newChat() {
    this.chatHistoryService.createNewConversation();
  }

  toggleAgenticAI() {
    this.useAgenticAI.update(current => !current);
  }

  onSubmit() {
    if (this.question.trim()) {
      this.onQuerySubmitted(this.question);
      this.question = '';
    }
  }

  getLastQuestion(response: QueryResponse | AgenticQueryResponse): string {
    const index = this.responses().indexOf(response);
    return this.questions[index] || 'Question';
  }

  isAgenticResponse(response: QueryResponse | AgenticQueryResponse): response is AgenticQueryResponse {
    return 'toolsUsed' in response;
  }

  onQuerySubmitted(question: string) {
    this.questions.push(question);
    this.isLoading.set(true);

    if (this.useAgenticAI()) {
      this.loadingMessage.set('🤖 Agentic AI processing...');
    } else {
      this.loadingMessage.set('📄 Searching documents...');
    }
    
    const request = {
      question,
      conversationId: this.conversationId,
      topK: 5
    };

    this.ragService.query(request)
      .pipe(
        timeout(30000), // 30 seconds timeout
        catchError(error => {
          console.error('Query error:', error);
          return of({
            answer: this.useAgenticAI() 
              ? 'The agentic AI service encountered an error. Please try again.' 
              : 'The search service encountered an error. Please try again.',
            conversationId: this.conversationId || 'error',
            citations: [],
            tokensUsed: 0
          } as QueryResponse);
        })
      )
      .subscribe({
      next: (response) => {
        this.responses.update(responses => [...responses, response]);
        this.currentCitations.set(response.citations);
        this.currentToolsUsed.set([]); // Tools info is in the response text for agentic mode
        this.conversationId = response.conversationId;
        this.chatHistoryService.addMessage(question, response);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Query error:', error);
        this.isLoading.set(false);
      }
    });
  }
}