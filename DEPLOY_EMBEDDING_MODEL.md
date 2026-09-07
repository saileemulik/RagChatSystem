# How to Deploy an Embedding Model in Azure OpenAI

## 🎯 Step-by-Step Guide to Create Embedding Model Deployment

### **Option 1: Azure Portal (Recommended - Visual Interface)**

#### Step 1: Navigate to Azure OpenAI Studio
1. **Go to**: https://oai.azure.com/
2. **Sign in** with your Azure account
3. **Select your resource**: `pstestopenaidply-xao3272r4u4hs`

#### Step 2: Create New Deployment
1. **Click "Deployments"** in the left menu
2. **Click "+ Create new deployment"** button
3. **Configure the deployment**:
   - **Model**: Select `text-embedding-ada-002` (or `text-embedding-3-small` for newer version)
   - **Deployment name**: `text-embedding-ada-002` (use exact name)
   - **Model version**: Select latest available
   - **Deployment type**: Standard
   - **Tokens per minute rate limit**: 120,000 (default)
4. **Click "Create"**
5. **Wait 2-5 minutes** for deployment to complete

#### Step 3: Verify Deployment
1. **Check deployment status**: Should show "Succeeded"
2. **Note the deployment name**: Must match your config exactly
3. **Test the endpoint** (optional)

---

### **Option 2: Azure Portal Resource Management**

#### Alternative Path via Azure Portal:
1. **Go to**: https://portal.azure.com
2. **Search for**: "Azure OpenAI"
3. **Select your resource**: `pstestopenaidply-xao3272r4u4hs`
4. **Click "Model deployments"** in left menu
5. **Click "Manage Deployments"** → Opens Azure OpenAI Studio
6. **Follow steps from Option 1**

---

### **Option 3: Azure CLI (Advanced Users)**

```bash
# Login to Azure
az login

# Set subscription (replace with your subscription ID)
az account set --subscription "9734ed68-621d-47ed-babd-269110dbacb1"

# Create embedding deployment
az cognitiveservices account deployment create \
  --resource-group "1-e115be62-playground-sandbox" \
  --account-name "pstestopenaidply-xao3272r4u4hs" \
  --deployment-name "text-embedding-ada-002" \
  --model-name "text-embedding-ada-002" \
  --model-version "2" \
  --model-format "OpenAI" \
  --sku-capacity 120 \
  --sku-name "Standard"
```

---

## 🎯 **Recommended Model Choices:**

### **Option A: Classic (Most Compatible)**
- **Model**: `text-embedding-ada-002`
- **Deployment Name**: `text-embedding-ada-002`
- **Pros**: Widely supported, stable
- **Cons**: Older model

### **Option B: Latest (Better Performance)**
- **Model**: `text-embedding-3-small`
- **Deployment Name**: `text-embedding-3-small`
- **Pros**: Newer, better quality, cheaper
- **Cons**: Requires config update

---

## ⚙️ **Update Your Configuration**

### If you choose `text-embedding-ada-002` (no change needed):
Your current config is correct:
```json
"EmbeddingDeploymentName": "text-embedding-ada-002"
```

### If you choose `text-embedding-3-small` (update needed):
Update your `appsettings.json`:
```json
"EmbeddingDeploymentName": "text-embedding-3-small"
```

---

## 🔍 **Verification Steps**

### Step 1: Check Deployment Status
1. **In Azure OpenAI Studio**: Deployments should show "Succeeded"
2. **Endpoint should be**: `https://pstestopenaidply-xao3272r4u4hs.openai.azure.com/openai/deployments/text-embedding-ada-002/embeddings`

### Step 2: Test with curl
```bash
curl -X POST "https://pstestopenaidply-xao3272r4u4hs.openai.azure.com/openai/deployments/text-embedding-ada-002/embeddings?api-version=2023-05-15" \
  -H "Content-Type: application/json" \
  -H "api-key: 6d62c4a878984156aa7f123059098ec2" \
  -d '{"input": "Hello world"}'
```

**Expected Response**: Array of 1536 numbers (embedding vector)

### Step 3: Test Your Application
1. **Restart your API**: `dotnet run`
2. **Upload a PDF** via `/api/ingest`
3. **Should see**: "Successfully ingested document with X chunks"
4. **Ask questions** via `/api/query`
5. **Should get**: Responses with citations

---

## 🚨 **Common Issues & Solutions**

### Issue: "Quota exceeded"
**Solution**: You might have reached your deployment limit. Delete unused deployments first.

### Issue: "Model not available"
**Solution**: Some models aren't available in all regions. Try `text-embedding-ada-002` first.

### Issue: "Deployment name already exists"
**Solution**: Use a different deployment name or delete the existing one.

### Issue: Still getting 404 after deployment
**Solution**: 
1. Wait 5-10 minutes after creation
2. Verify exact deployment name spelling
3. Check the deployment is in "Succeeded" state

---

## 💡 **Quick Start Recommendation**

**For fastest setup:**
1. **Use Azure OpenAI Studio** (Option 1)
2. **Deploy `text-embedding-ada-002`** (most compatible)
3. **Keep default settings**
4. **Wait for "Succeeded" status**
5. **Restart your application**
6. **Test with PDF upload**

Your embedding deployment will work alongside your existing chat deployment to enable full RAG functionality!