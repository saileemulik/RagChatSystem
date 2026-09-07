# Quick Start Guide

## Step 1: Configure Azure Services

Edit `appsettings.json` in the `RagChatSystem.Api` folder and add your Azure credentials:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
    "ApiKey": "YOUR-API-KEY",
    "DeploymentName": "gpt-4",
    "EmbeddingDeploymentName": "text-embedding-ada-002"
  },
  "AzureSearch": {
    "Endpoint": "https://YOUR-SEARCH-SERVICE.search.windows.net",
    "ApiKey": "YOUR-SEARCH-KEY",
    "IndexName": "rag-documents"
  },
  "AzureBlobStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=YOUR-ACCOUNT;AccountKey=YOUR-KEY;EndpointSuffix=core.windows.net",
    "ContainerName": "pdf-documents"
  }
}
```

## Step 2: Run the Application

```bash
cd "c:\MY FILES\genai-casestudy\RagChatSystem.Api"
dotnet run
```

The API will start at: `https://localhost:7000` (or check console for the actual port)

Swagger UI: `https://localhost:7000/swagger`

## Step 3: Test with Postman

### Test 1: Upload a PDF

1. **Method**: POST
2. **URL**: `https://localhost:7000/api/ingest`
3. **Body**: form-data
   - Key: `file` (File) → Select a PDF
   - Key: `metadata` (Text) → `author=Test;category=Demo`
4. **Send**

Expected Response:
```json
{
  "documentId": "guid-here",
  "fileName": "your-file.pdf",
  "chunksCreated": 15,
  "status": "Success",
  "message": "Document ingested successfully with 15 chunks"
}
```

### Test 2: Ask a Question

1. **Method**: POST
2. **URL**: `https://localhost:7000/api/query`
3. **Headers**: `Content-Type: application/json`
4. **Body** (raw JSON):
```json
{
  "question": "What is this document about?",
  "topK": 5
}
```
5. **Send**

Expected Response:
```json
{
  "answer": "Based on the documents...",
  "conversationId": "guid-for-follow-ups",
  "citations": [
    {
      "fileName": "your-file.pdf",
      "content": "Relevant chunk...",
      "score": 0.85
    }
  ],
  "tokensUsed": 0
}
```

### Test 3: Follow-up Question (Multi-turn)

Use the `conversationId` from Test 2:
```json
{
  "question": "Can you explain that in more detail?",
  "conversationId": "guid-from-previous-response",
  "topK": 5
}
```

## Import Postman Collection

Import `RAGChatSystem.postman_collection.json` into Postman for pre-configured requests.

## Troubleshooting

**"SSL certificate problem"**: 
- In Postman: Settings → General → SSL certificate verification → OFF

**"Configuration missing" error**:
- Verify all Azure settings in `appsettings.json`

**"Index creation failed"**:
- Ensure your Azure Search service supports vector search (Basic tier or higher)

## What Happens Under the Hood

### Ingestion Pipeline:
1. PDF uploaded to Azure Blob Storage
2. Text extracted using iText7
3. Text chunked into ~1000 character segments with 200 char overlap
4. Each chunk embedded using Azure OpenAI (text-embedding-ada-002)
5. Chunks + embeddings indexed in Azure Cognitive Search

### Query Pipeline:
1. Question embedded using Azure OpenAI
2. Vector search in Azure Cognitive Search finds top-K similar chunks
3. Semantic Kernel builds prompt with retrieved context
4. Azure OpenAI generates answer based on context
5. Response includes answer + citations with source references

## Next Steps

- Upload multiple PDFs to build your knowledge base
- Test different questions to see the RAG system in action
- Adjust chunk size in `appsettings.json` if needed
- Monitor Azure costs (embeddings + chat completion tokens)
