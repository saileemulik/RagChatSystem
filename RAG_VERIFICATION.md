# RAG Verification Guide - Ensure Document-Based Responses

## 🔍 How to Verify You're Getting Document-Based Responses (Not Just Direct AI Chat)

### ✅ Step 1: Check Current Configuration
Your configuration looks correct for RAG:
- ✅ Embedding Model: `text-embedding-ada-002`
- ✅ Azure Search: Configured
- ✅ Blob Storage: Configured

### ✅ Step 2: Verify RAG Workflow

#### Test 1: Upload a Document First
**CRITICAL**: You MUST upload a document before asking questions.

```
POST /api/ingest
Content-Type: multipart/form-data

file: [Upload a PDF with specific content you can verify]
```

Expected Response:
```json
{
  "documentId": "guid",
  "fileName": "your-file.pdf", 
  "chunksCreated": 15,  // ← Should be > 0
  "status": "Success",
  "message": "Document ingested successfully with 15 chunks"
}
```

#### Test 2: Ask Questions About Your Document
```json
{
  "question": "What is the main topic of the uploaded document?",
  "topK": 5
}
```

**Document-based response will include:**
- ✅ Answer based on your uploaded content
- ✅ `citations` array with source chunks
- ✅ `fileName` references in citations

**Direct AI response (wrong) would have:**
- ❌ Generic answer not from your document
- ❌ Empty `citations` array
- ❌ No file references

### 🔍 Step 3: Verify Response Structure

**✅ CORRECT RAG Response:**
```json
{
  "answer": "Based on the uploaded document, the main topic is...", 
  "conversationId": "guid",
  "citations": [
    {
      "fileName": "your-actual-filename.pdf",  // ← Your file
      "content": "Actual text from your document...",  // ← Your content
      "score": 0.85
    }
  ]
}
```

**❌ WRONG Direct AI Response:**
```json
{
  "answer": "I don't have access to any uploaded documents...",
  "citations": []  // ← Empty = not using documents
}
```

### 🧪 Step 4: Test Document-Specific Content

Upload a document and ask very specific questions that only your document would know:
- "What is the exact title mentioned in the document?"
- "What specific data/numbers are mentioned?"
- "Who is the author mentioned in the document?"

If responses contain your actual document content = ✅ RAG working
If responses are generic = ❌ Not using documents

### 🚨 Step 5: Check for These Warning Signs

**Warning signs you're NOT using documents:**
- Empty citations array
- Generic AI responses
- "I don't have information about..." messages
- No file names in responses

**Good signs you ARE using documents:**
- Citations with your file names
- Specific content from your uploads
- References to document sections
- Scores in citations (similarity scores)

---

## 🛠️ Troubleshooting

### Issue: Empty Citations
**Problem**: Not finding documents
**Solution**: Check if documents were actually uploaded and indexed

### Issue: Generic Responses  
**Problem**: Using direct AI instead of RAG
**Solution**: Verify embedding model is deployed and working

### Issue: "No relevant information found"
**Problem**: Question doesn't match document content
**Solution**: Ask questions directly related to uploaded content