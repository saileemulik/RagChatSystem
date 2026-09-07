using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using RagChatSystem.Api.Models;
 
namespace RagChatSystem.Api.Services;
 
public class RagOrchestrationService : IRagOrchestrationService
{
    private readonly Kernel _kernel;
    private readonly IAzureBlobStorageService _blobStorageService;
    private readonly IDocumentProcessingService _documentProcessingService;
    private readonly IAzureSearchService _searchService;
    private readonly IChatCompletionService _chatService;
    private readonly ILogger<RagOrchestrationService> _logger;

 
    public RagOrchestrationService(
        Kernel kernel,
        IAzureBlobStorageService blobStorageService,
        IDocumentProcessingService documentProcessingService,
        IAzureSearchService searchService,
        ILogger<RagOrchestrationService> logger)
    {
        _kernel = kernel;
        _blobStorageService = blobStorageService;
        _documentProcessingService = documentProcessingService;
        _searchService = searchService;
        _logger = logger;
        _chatService = kernel.GetRequiredService<IChatCompletionService>();
    }
 
    public async Task<IngestResponse> IngestDocumentAsync(
        Stream fileStream,
        string fileName,
        Dictionary<string, string> metadata)
    {
        var documentId = Guid.NewGuid().ToString();
       
        try
        {
            _logger.LogInformation("Starting ingestion for document {FileName}", fileName);
 
            // Step 1: Upload to Blob Storage
            var blobName = await _blobStorageService.UploadDocumentAsync(fileStream, fileName, documentId);
            fileStream.Position = 0;
 
            // Step 2: Extract and chunk text
            var chunks = await _documentProcessingService.ProcessDocumentAsync(fileStream, fileName, documentId, metadata);
 
            // Step 3: Index in Azure Search
            await _searchService.IndexDocumentChunksAsync(chunks);
 
            _logger.LogInformation("Successfully ingested document {FileName} with {ChunkCount} chunks",
                fileName, chunks.Count);

            var response = new IngestResponse
            {
                DocumentId = documentId,
                FileName = fileName,
                ChunksCreated = chunks.Count,
                Status = "Success",
                Message = $"Document ingested successfully with {chunks.Count} chunks"
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ingesting document {FileName}", fileName);
            return new IngestResponse
            {
                DocumentId = documentId,
                FileName = fileName,
                Status = "Failed",
                Message = $"Error: {ex.Message}"
            };
        }
    }
 
    public async Task<QueryResponse> QueryAsync(string question, string? conversationId, int topK)
    {
        const int maxRetries = 3;
       
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("Processing query: {Question}", question);
 
                var searchResults = await _searchService.SearchTextAsync(question, topK);
 
                if (!searchResults.Any())
                {
                    return new QueryResponse
                    {
                        Answer = "I couldn't find any relevant information in the documents to answer your question. Please try different keywords or upload relevant documents.",
                        ConversationId = conversationId ?? Guid.NewGuid().ToString(),
                        Citations = new List<Citation>()
                    };
                }
 
                // Limit context to reduce tokens
                var context = string.Join("\n\n", searchResults.Take(2).Select(r =>
                    $"[Source: {r.FileName}]\n{r.Content.Substring(0, Math.Min(300, r.Content.Length))}"));
 
                conversationId ??= Guid.NewGuid().ToString();
               
                // Use fresh chat history to avoid token buildup
                var chatHistory = new ChatHistory();
               
                var systemPrompt = @"Answer based on context. Be concise.
 
Context:
" + context;
 
                chatHistory.AddSystemMessage(systemPrompt);
                chatHistory.AddUserMessage(question);
 
                var result = await _chatService.GetChatMessageContentAsync(chatHistory);
                var answer = result.Content ?? "I couldn't generate an answer.";
 
                var citations = searchResults.Take(1).Select(r => new Citation
                {
                    FileName = r.FileName,
                    Content = r.Content.Length > 200 ? r.Content.Substring(0, 200) + "..." : r.Content,
                    Score = r.Score
                }).ToList();
 
                _logger.LogInformation("Successfully answered query for conversation {ConversationId}", conversationId);
 
                return new QueryResponse
                {
                    Answer = answer,
                    ConversationId = conversationId,
                    Citations = citations,
                    TokensUsed = 0
                };
            }
            catch (Microsoft.SemanticKernel.HttpOperationException ex) when (ex.Message.Contains("429") && attempt < maxRetries - 1)
            {
                _logger.LogWarning("Rate limit hit, waiting 60 seconds (attempt {Attempt})", attempt + 1);
                await Task.Delay(60000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing query");
                throw;
            }
        }
       
        throw new InvalidOperationException("Rate limit exceeded after retries");
    }

    public async Task DeleteDocumentAsync(string documentId)
    {
        try
        {
            _logger.LogInformation("Deleting document {DocumentId}", documentId);

            // Delete from Azure Search
            await _searchService.DeleteDocumentChunksAsync(documentId);

            // Delete from Blob Storage - find blob by documentId prefix
            var blobNames = await _blobStorageService.GetAllBlobNamesAsync();
            var documentBlobs = blobNames.Where(name => name.StartsWith($"{documentId}/")).ToList();
            
            foreach (var blobName in documentBlobs)
            {
                await _blobStorageService.DeleteDocumentAsync(blobName);
            }

            _logger.LogInformation("Successfully deleted document {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            throw;
        }
    }

    public async Task<List<IngestResponse>> GetDocumentsAsync()
    {
        return await Task.FromResult(new List<IngestResponse>());
    }

    public async Task<DocumentListResponse> GetDocumentListAsync()
    {
        try
        {
            var blobNames = await _blobStorageService.GetAllBlobNamesAsync();
            var documents = new List<DocumentInfo>();

            foreach (var blobName in blobNames)
            {
                var parts = blobName.Split('/');
                if (parts.Length >= 2)
                {
                    var documentId = parts[0];
                    var fileName = string.Join("/", parts.Skip(1));
                    
                    documents.Add(new DocumentInfo
                    {
                        DocumentId = documentId,
                        FileName = fileName,
                        UploadedAt = DateTime.UtcNow // Note: Blob storage doesn't preserve original upload time in this implementation
                    });
                }
            }

            return new DocumentListResponse
            {
                Documents = documents,
                TotalCount = documents.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document list from blob storage");
            return new DocumentListResponse
            {
                Documents = new List<DocumentInfo>(),
                TotalCount = 0
            };
        }
    }
}