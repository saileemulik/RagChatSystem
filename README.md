# RAG Chat System - AI-Powered PDF Q&A with Semantic Kernel

A production-ready .NET 9 Web API that implements a Retrieval-Augmented Generation (RAG) pattern using Semantic Kernel, Azure OpenAI, and Azure Cognitive Search. Upload PDFs, chunk and embed their content, then ask questions to get AI-powered answers with citations.

## 🏗️ Architecture

```
PDF Upload → Text Extraction → Chunking → Embedding → Azure Cognitive Search
                                                               ↓
User Question → Embedding → Vector Search → Context Building → Azure OpenAI → Answer + Citations

<img width="1308" height="621" alt="image" src="https://github.com/user-attachments/assets/d3bc4205-e49f-45f5-ac16-bf01a4064606" />

```
## 🖥️ Screenshots

### 🏠 RAG Frontend Home

<img width="1920" height="1080" alt="RAG Frontend Home" src="https://github.com/user-attachments/assets/0acbdf7e-2e9b-47dc-bb17-cb1aa6892778" />

### 📤 Document Upload

<img width="1920" height="1080" alt="RAG Document Upload Feature" src="https://github.com/user-attachments/assets/640be1fe-34a4-404d-bc14-28ebe9eeaaf2" />

### 📄 Document Upload – Processing

<img width="1920" height="1080" alt="RAG Document Upload Feature 2" src="https://github.com/user-attachments/assets/2bdc35f0-aa5d-43fe-9422-24bd0429cb76" />

### 📚 Document Management

<img width="1920" height="1080" alt="RAG Document Management Feature" src="https://github.com/user-attachments/assets/ac181344-0d5d-41f4-832b-b4bbff0fa3a4" />

### 🤖 RAG Chatbot

<img width="1920" height="1080" alt="RAG Chatbot" src="https://github.com/user-attachments/assets/d744c70c-bbd0-4adc-b082-6775a58de652" />


<img width="1920" height="1080" alt="RAG Chatbot 2" src="https://github.com/user-attachments/assets/0bcfc4bc-13db-4269-841e-61a9ee8b1e6b" />




## 🚀 Features

- **PDF Ingestion Pipeline**: Automatic text extraction, chunking, and embedding generation
- **Vector Search**: Fast semantic search using Azure Cognitive Search
- **Semantic Kernel Orchestration**: Modular and maintainable AI workflow
- **Conversation Memory**: Multi-turn Q&A with conversation history
- **Citations**: Answers include source references from documents
- **RESTful API**: Clean endpoints for integration

## 📋 Prerequisites

- .NET 9 SDK
- Azure OpenAI account with:
  - GPT-4 (or GPT-3.5-turbo) deployment
  - text-embedding-ada-002 deployment
- Azure Cognitive Search service
- Azure Blob Storage account

## ⚙️ Configuration

Update `appsettings.json` with your Azure credentials:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-openai-resource.openai.azure.com/",
    "ApiKey": "your-openai-api-key",
    "DeploymentName": "gpt-4",
    "EmbeddingDeploymentName": "text-embedding-ada-002"
  },
  "AzureSearch": {
    "Endpoint": "https://your-search-service.search.windows.net",
    "ApiKey": "your-search-api-key",
    "IndexName": "rag-documents"
  },
  "AzureBlobStorage": {
    "ConnectionString": "your-blob-storage-connection-string",
    "ContainerName": "pdf-documents"
  },
  "DocumentProcessing": {
    "ChunkSize": 1000,
    "ChunkOverlap": 200,
    "TopK": 5
  }
}
```

## 🔧 Installation & Setup

1. **Clone and restore packages**:
```bash
cd "c:\MY FILES\genai-casestudy\RagChatSystem.Api"
dotnet restore
```

2. **Update configuration**: Edit `appsettings.json` with your Azure credentials

3. **Run the application**:
```bash
dotnet run
```

The API will start at `https://localhost:7000` (or the port shown in the console)

## 📡 API Endpoints

### 1. Ingest PDF Document
**POST** `/api/ingest`

Uploads a PDF, extracts text, chunks it, generates embeddings, and indexes in Azure Cognitive Search.

**Request** (multipart/form-data):
- `file`: PDF file
- `metadata` (optional): Key-value pairs separated by semicolons (e.g., `author=John Doe;category=Technical`)

**Response**:
```json
{
  "documentId": "guid",
  "fileName": "document.pdf",
  "chunksCreated": 15,
  "status": "Success",
  "message": "Document ingested successfully with 15 chunks"
}
```

### 2. Query Documents
**POST** `/api/query`

Ask questions about ingested documents and get AI-powered answers with citations.

**Request** (application/json):
```json
{
  "question": "What are the main benefits of using Semantic Kernel?",
  "conversationId": "optional-conversation-id",
  "topK": 5
}
```

**Response**:
```json
{
  "answer": "Based on the documents, the main benefits include...",
  "conversationId": "conversation-guid",
  "citations": [
    {
      "fileName": "document.pdf",
      "content": "Relevant chunk content...",
      "score": 0.85
    }
  ],
  "tokensUsed": 0
}
```

## 🧪 Testing with Postman

### Ingest a PDF:
1. Set method to **POST**
2. URL: `https://localhost:7000/api/ingest`
3. Body → form-data
4. Add key `file` (type: File) and select a PDF
5. (Optional) Add key `metadata` (type: Text) with value like `author=Test;category=Demo`
6. Send request

### Query Documents:
1. Set method to **POST**
2. URL: `https://localhost:7000/api/query`
3. Headers: `Content-Type: application/json`
4. Body → raw (JSON):
```json
{
  "question": "What is this document about?",
  "topK": 5
}
```
5. Send request

### Multi-turn Conversation:
Use the `conversationId` from the first response in subsequent queries to maintain context:
```json
{
  "question": "Tell me more about that",
  "conversationId": "from-previous-response",
  "topK": 5
}
```

## 📁 Project Structure

```
RagChatSystem.Api/
├── Controllers/
│   ├── IngestController.cs      # PDF upload endpoint
│   └── QueryController.cs        # Question answering endpoint
├── Models/
│   ├── AppSettings.cs            # Configuration models
│   ├── DocumentChunk.cs          # Document chunk entity
│   ├── IngestRequest.cs          # Ingestion DTOs
│   ├── IngestResponse.cs
│   ├── QueryRequest.cs           # Query DTOs
│   └── QueryResponse.cs
├── Services/
│   ├── BlobStorageService.cs     # Azure Blob Storage operations
│   ├── DocumentProcessingService.cs  # PDF text extraction & chunking
│   ├── EmbeddingService.cs       # Azure OpenAI embeddings
│   ├── SearchService.cs          # Azure Cognitive Search operations
│   └── RagOrchestrationService.cs    # Main RAG workflow coordinator
├── Program.cs                    # Startup & DI configuration
└── appsettings.json             # Configuration
```

## 🔑 Key Components

### Semantic Kernel Integration
- **Kernel Configuration**: Manages Azure OpenAI chat completion and embeddings
- **Memory Management**: Maintains conversation history for multi-turn dialogues
- **Service Orchestration**: Coordinates the entire RAG pipeline

### Document Processing
- Extracts text from PDFs using iText7
- Chunks text with configurable size and overlap
- Preserves sentence boundaries for better context

### Vector Search
- Generates embeddings using Azure OpenAI
- Performs semantic search in Azure Cognitive Search
- Returns top-K most relevant chunks

### RAG Pipeline
1. **Ingestion**: PDF → Extract → Chunk → Embed → Index
2. **Query**: Question → Embed → Search → Build Context → Generate Answer

## 🛠️ Customization

### Adjust Chunk Size
In `appsettings.json`:
```json
"DocumentProcessing": {
  "ChunkSize": 1500,      // Increase for larger chunks
  "ChunkOverlap": 300,    // Increase for more context retention
  "TopK": 3               // Number of chunks to retrieve
}
```

### Change AI Models
Update deployment names in `appsettings.json`:
```json
"AzureOpenAI": {
  "DeploymentName": "gpt-3.5-turbo",  // Use GPT-3.5 for cost savings
  "EmbeddingDeploymentName": "text-embedding-3-small"
}
```


## 📝 Notes

- The search index is automatically created on startup
- Conversation memory is in-memory (lost on restart)
- Embeddings use 1536 dimensions (text-embedding-ada-002)
- File size limits can be configured in `appsettings.json`

## 🐛 Troubleshooting

**Error: "AzureOpenAI configuration is missing"**
- Ensure `appsettings.json` has all required Azure settings

**Error: "Index creation failed"**
- Verify Azure Cognitive Search endpoint and API key
- Check that your search service tier supports vector search

**Error: "Blob storage connection failed"**
- Validate your Azure Blob Storage connection string
- Ensure the container name is valid (lowercase, no spaces)

## 📄 License

MIT License - Feel free to use in your projects!
