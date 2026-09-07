# Sample Questions for Testing RAG Chat System API

## 🧪 **Test Questions for /api/query Endpoint**

Based on your uploaded document "Whiz-Cheat-Sheet-Azure-AI-Fundamentals-AI-900-v2.pdf", here are sample questions to test your RAG system:

### **✅ Category 1: Azure AI Services**

```json
{
  "question": "What are the different Azure AI Services mentioned in the document?",
  "topK": 5
}
```

```json
{
  "question": "What is Azure Machine Learning used for?",
  "topK": 3
}
```

```json
{
  "question": "What cognitive capabilities does Azure AI Services provide?",
  "topK": 5
}
```

### **✅ Category 2: Facial Recognition & Computer Vision**

```json
{
  "question": "What facial recognition capabilities are available in Azure?",
  "topK": 4
}
```

```json
{
  "question": "How does facial recognition work for identity verification?",
  "topK": 5
}
```

```json
{
  "question": "What can computer vision detect in images?",
  "topK": 4
}
```

### **✅ Category 3: Language and Translation Services**

```json
{
  "question": "What translation services are available in Azure?",
  "topK": 5
}
```

```json
{
  "question": "How does sentiment analysis work in Azure?",
  "topK": 4
}
```

```json
{
  "question": "What is Language Studio Platform used for?",
  "topK": 3
}
```

### **✅ Category 4: Document Processing**

```json
{
  "question": "How can Azure translate documents while preserving layout?",
  "topK": 4
}
```

```json
{
  "question": "What document analysis capabilities are mentioned?",
  "topK": 5
}
```

### **✅ Category 5: AI-900 Exam Content**

```json
{
  "question": "What are the key topics covered for AI-900 exam?",
  "topK": 5
}
```

```json
{
  "question": "What machine learning concepts are explained?",
  "topK": 4
}
```

### **✅ Category 6: Multi-turn Conversation Testing**

**First Question:**
```json
{
  "question": "What are Azure AI Services?",
  "topK": 5
}
```

**Follow-up Question (use conversationId from first response):**
```json
{
  "question": "Can you give me more details about the cognitive capabilities?",
  "conversationId": "PASTE-CONVERSATION-ID-HERE",
  "topK": 3
}
```

**Third Question (same conversationId):**
```json
{
  "question": "How do these relate to the AI-900 exam?",
  "conversationId": "SAME-CONVERSATION-ID",
  "topK": 4
}
```

---

## 🔍 **Testing Different Response Patterns**

### **Questions That Should Find Good Matches:**
```json
{
  "question": "Azure Machine Learning",
  "topK": 3
}
```

```json
{
  "question": "sentiment analysis methods",
  "topK": 4
}
```

### **Questions That Might Return "No Information Found":**
```json
{
  "question": "What is the weather like today?",
  "topK": 5
}
```

```json
{
  "question": "How to cook pasta?",
  "topK": 3
}
```

### **Edge Case Testing:**
```json
{
  "question": "",
  "topK": 5
}
```

```json
{
  "question": "AI",
  "topK": 10
}
```

---

## 🎯 **Expected Response Patterns**

### **✅ Good RAG Response Should Include:**
- **Answer**: Content based on your document
- **Citations**: Array with 3-5 items
- **FileName**: "Whiz-Cheat-Sheet-Azure-AI-Fundamentals-AI-900-v2.pdf"
- **Content**: Actual text snippets from your PDF
- **Scores**: Relevance scores (higher = more relevant)

### **✅ Example Good Response:**
```json
{
  "answer": "Based on the document, Azure AI Services utilize cognitive capabilities to comprehend diverse content types...",
  "conversationId": "guid",
  "citations": [
    {
      "fileName": "Whiz-Cheat-Sheet-Azure-AI-Fundamentals-AI-900-v2.pdf",
      "content": "Azure AI Services: Utilizes cognitive capabilities...",
      "score": 4.5
    }
  ]
}
```

### **⚠️ Poor Response Would Have:**
- Empty citations array
- Generic AI knowledge not from your document
- No file name references

---

## 🚀 **Quick Test Sequence**

**Step 1:** Test basic functionality
```json
{
  "question": "What is Azure Machine Learning?",
  "topK": 5
}
```

**Step 2:** Test specific content
```json
{
  "question": "facial recognition identity verification",
  "topK": 4
}
```

**Step 3:** Test conversation memory
```json
{
  "question": "Tell me about sentiment analysis",
  "topK": 3
}
```

Then use the conversationId from Step 3:
```json
{
  "question": "What tools are used for that?",
  "conversationId": "from-step-3-response",
  "topK": 3
}
```

**Step 4:** Test edge case
```json
{
  "question": "Python programming",
  "topK": 5
}
```

This should show how your system handles questions not in your document.

---

## 💡 **Pro Tips for Testing:**

1. **Copy conversationId** from responses for multi-turn testing
2. **Check citations array** to verify document usage
3. **Try variations** of the same question
4. **Test with different topK values** (1, 3, 5, 7)
5. **Mix specific and general** questions

Your document contains rich content about Azure AI services, so these questions should give you comprehensive test coverage! 🎯