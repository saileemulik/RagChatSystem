# How to Create Azure Cognitive Search Service

## Option 1: Using Azure Portal (Easiest)

### Step 1: Create Azure Cognitive Search Service

1. **Go to Azure Portal**: https://portal.azure.com
2. **Click "Create a resource"**
3. **Search for "Azure Cognitive Search"** or "Azure AI Search"
4. **Click "Create"**

### Step 2: Configure the Service

Fill in the following details:

- **Subscription**: Select your Azure subscription
- **Resource Group**: Create new or select existing (e.g., `rg-rag-chat-system`)
- **Service Name**: Choose a unique name (e.g., `rag-search-service-demo`)
  - This will be part of your endpoint: `https://YOUR-SERVICE-NAME.search.windows.net`
- **Location**: Choose a region close to your Azure OpenAI resource (e.g., East US)
- **Pricing Tier**: 
  - **Basic** (Recommended for development) - $75/month, supports vector search
  - **Free** - No cost but limited features and NO vector search support
  - **Standard** - For production workloads

⚠️ **IMPORTANT**: You MUST choose **Basic tier or higher** for vector search support!

5. **Click "Review + Create"**
6. **Click "Create"** (deployment takes 2-5 minutes)

### Step 3: Get Your Endpoint and API Key

Once deployment completes:

1. **Go to your Search service** in the Azure Portal
2. **Copy the Endpoint**:
   - Look at the **Overview** page
   - Copy the **Url** (e.g., `https://rag-search-service-demo.search.windows.net`)

3. **Get the API Key**:
   - In the left menu, click **"Keys"**
   - Copy either **"Primary admin key"** or **"Secondary admin key"**
   - These are full access keys for management operations

### Step 4: Update Your Configuration

Update `appsettings.json`:

```json
"AzureSearch": {
  "Endpoint": "https://YOUR-SERVICE-NAME.search.windows.net",
  "ApiKey": "YOUR-ADMIN-KEY-HERE",
  "IndexName": "rag-documents"
}
```

---

## Option 2: Using Azure CLI (For Automation)

### Prerequisites
Install Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli

### Commands

```bash
# Login to Azure
az login

# Set your subscription
az account set --subscription "YOUR-SUBSCRIPTION-ID"

# Create a resource group (if you don't have one)
az group create --name rg-rag-chat-system --location eastus

# Create Azure Cognitive Search service
az search service create \
  --name rag-search-service-demo \
  --resource-group rg-rag-chat-system \
  --sku Basic \
  --location eastus

# Get the endpoint (automatically generated)
az search service show \
  --name rag-search-service-demo \
  --resource-group rg-rag-chat-system \
  --query "hostName" -o tsv

# Get the admin key
az search admin-key show \
  --service-name rag-search-service-demo \
  --resource-group rg-rag-chat-system \
  --query "primaryKey" -o tsv
```

---

## Pricing Tiers Comparison

| Tier | Price/Month | Vector Search | Storage | Max Indexes |
|------|-------------|---------------|---------|-------------|
| Free | $0 | ❌ No | 50 MB | 3 |
| Basic | ~$75 | ✅ Yes | 2 GB | 15 |
| Standard S1 | ~$250 | ✅ Yes | 25 GB | 50 |
| Standard S2 | ~$1000 | ✅ Yes | 100 GB | 200 |

**For this RAG project**: Use **Basic** tier minimum (required for vector search).

---

## Verify Your Configuration

After creating the service, test the endpoint:

```bash
# Test with curl (Windows PowerShell)
curl -H "api-key: YOUR-API-KEY" https://YOUR-SERVICE-NAME.search.windows.net/indexes?api-version=2023-11-01

# You should see an empty array [] if the service is working
```

---

## Common Issues

### ❌ "Vector search not supported"
- **Solution**: Upgrade to Basic tier or higher (Free tier doesn't support vectors)

### ❌ "Unauthorized" error
- **Solution**: Check your API key is correct and has admin permissions

### ❌ "Service name already exists"
- **Solution**: Choose a different unique name (must be globally unique)

### ❌ "Quota exceeded"
- **Solution**: You can only have 1 Free tier search service per subscription

---

## Security Best Practices

1. **Use Managed Identity** (for production):
   - Configure your API to use Azure Managed Identity instead of API keys
   - More secure and no key management needed

2. **Use Query Keys** (for read-only operations):
   - For production queries, create query keys instead of using admin keys
   - Admin keys = full access, Query keys = read-only

3. **Store keys in Azure Key Vault**:
   - Never commit API keys to source control
   - Use environment variables or Azure Key Vault

---

## Next Steps

Once you have your endpoint and API key:

1. ✅ Update `appsettings.json` with your values
2. ✅ Run your API: `dotnet run`
3. ✅ The index will be created automatically on first startup
4. ✅ Test with Swagger UI

The application will automatically:
- Create the search index with vector fields
- Configure the HNSW algorithm for vector search
- Set up the schema for document chunks
