export interface QueryRequest {
  question: string;
  conversationId?: string;
  topK?: number;
}

export interface AgenticQueryRequest {
  question: string;
  conversationId?: string;
  topK?: number;
  useWebSearch?: boolean;
  useCalculator?: boolean;
}

export interface Citation {
  fileName: string;
  content: string;
  score: number;
}

export interface ToolUsage {
  toolName: string;
  input: string;
  output: string;
  executedAt: string;
}

export interface QueryResponse {
  answer: string;
  conversationId: string;
  citations: Citation[];
  tokensUsed: number;
}

export interface AgenticQueryResponse {
  answer: string;
  conversationId: string;
  citations: Citation[];
  toolsUsed: ToolUsage[];
  tokensUsed: number;
}

export interface IngestResponse {
  documentId: string;
  fileName: string;
  chunksCreated: number;
  status: string;
  message: string;
}

export interface DocumentInfo {
  documentId: string;
  fileName: string;
  uploadedAt: string;
  content?: string;
}

export interface ChatMessage {
  id: string;
  question: string;
  response: QueryResponse;
  timestamp: Date;
}

export interface Conversation {
  id: string;
  title: string;
  messages: ChatMessage[];
  lastUpdated: Date;
  createdAt: Date;
}