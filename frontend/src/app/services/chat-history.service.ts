import { Injectable, signal } from '@angular/core';
import { ChatMessage, QueryResponse, Conversation } from '../models/query.model';

@Injectable({
  providedIn: 'root'
})
export class ChatHistoryService {
  private _conversations = signal<Conversation[]>([]);
  private _activeConversationId = signal<string | null>(null);
  private readonly STORAGE_KEY = 'rag-conversations';

  constructor() {
    this.loadFromStorage();
  }

  get conversations() {
    return this._conversations.asReadonly();
  }

  get activeConversationId() {
    return this._activeConversationId.asReadonly();
  }

  get activeConversation() {
    const id = this._activeConversationId();
    return id ? this._conversations().find(c => c.id === id) : null;
  }

  createNewConversation(): string {
    const id = crypto.randomUUID();
    const conversation: Conversation = {
      id,
      title: 'New Chat',
      messages: [],
      lastUpdated: new Date(),
      createdAt: new Date()
    };

    this._conversations.update(convs => [conversation, ...convs]);
    this._activeConversationId.set(id);
    this.saveToStorage();
    return id;
  }

  selectConversation(id: string) {
    this._activeConversationId.set(id);
  }

  addMessage(question: string, response: QueryResponse) {
    let conversationId = this._activeConversationId();

    if (!conversationId) {
      conversationId = this.createNewConversation();
    }

    const message: ChatMessage = {
      id: crypto.randomUUID(),
      question,
      response,
      timestamp: new Date()
    };

    this._conversations.update(convs =>
      convs.map(conv => {
        if (conv.id === conversationId) {
          const updatedConv = {
            ...conv,
            messages: [...conv.messages, message],
            lastUpdated: new Date(),
            title: conv.messages.length === 0 ? question.substring(0, 30) + '...' : conv.title
          };
          return updatedConv;
        }
        return conv;
      })
    );

    this.saveToStorage();
  }

  deleteConversation(id: string) {
    this._conversations.update(convs => convs.filter(c => c.id !== id));
    if (this._activeConversationId() === id) {
      const remaining = this._conversations();
      this._activeConversationId.set(remaining.length > 0 ? remaining[0].id : null);
    }
    this.saveToStorage();
  }

  clearAllHistory() {
    this._conversations.set([]);
    this._activeConversationId.set(null);
    localStorage.removeItem(this.STORAGE_KEY);
  }

  private loadFromStorage() {
    const stored = localStorage.getItem(this.STORAGE_KEY);
    if (stored) {
      try {
        const data = JSON.parse(stored);
        const conversations = data.conversations?.map((conv: any) => ({
          ...conv,
          createdAt: new Date(conv.createdAt),
          lastUpdated: new Date(conv.lastUpdated),
          messages: conv.messages.map((msg: any) => ({
            ...msg,
            timestamp: new Date(msg.timestamp)
          }))
        })) || [];

        this._conversations.set(conversations);
        this._activeConversationId.set(data.activeConversationId || null);
      } catch (e) {
        console.error('Failed to load conversations:', e);
      }
    }
  }

  private saveToStorage() {
    const data = {
      conversations: this._conversations(),
      activeConversationId: this._activeConversationId()
    };
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(data));
  }
}
