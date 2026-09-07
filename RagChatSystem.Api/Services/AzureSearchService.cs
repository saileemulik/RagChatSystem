using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using RagChatSystem.Api.Models;

namespace RagChatSystem.Api.Services;

public interface IAzureSearchService
{
    Task InitializeIndexAsync();
    Task IndexDocumentChunksAsync(List<DocumentChunk> chunks);
    Task<List<SearchResult>> SearchTextAsync(string query, int topK);
    Task DeleteDocumentChunksAsync(string documentId);
}

public class AzureSearchService : IAzureSearchService
{
    private readonly SearchClient _searchClient;
    private readonly SearchIndexClient _indexClient;
    private readonly AzureSearchSettings _settings;
    private readonly ILogger<AzureSearchService> _logger;

    public AzureSearchService(
        IConfiguration configuration,
        ILogger<AzureSearchService> logger)
    {
        _settings = configuration.GetSection("AzureSearch").Get<AzureSearchSettings>()
            ?? throw new InvalidOperationException("AzureSearch configuration is missing");

        var credential = new AzureKeyCredential(_settings.ApiKey);
        _indexClient = new SearchIndexClient(new Uri(_settings.Endpoint), credential);
        _searchClient = _indexClient.GetSearchClient(_settings.IndexName);
        _logger = logger;
    }

    public async Task InitializeIndexAsync()
    {
        try
        {
            var definition = new SearchIndex(_settings.IndexName)
            {
                Fields = new List<SearchField>
                {
                    new SearchField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
                    new SearchField("documentId", SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                    new SearchField("fileName", SearchFieldDataType.String) { IsFilterable = true, IsSortable = true },
                    new SearchField("content", SearchFieldDataType.String) { IsSearchable = true },
                    new SearchField("chunkIndex", SearchFieldDataType.Int32) { IsFilterable = true, IsSortable = true },
                    new SearchField("uploadedAt", SearchFieldDataType.DateTimeOffset) { IsFilterable = true, IsSortable = true }
                }
            };

            await _indexClient.CreateOrUpdateIndexAsync(definition);
            _logger.LogInformation("Azure Search index {IndexName} created/updated successfully", _settings.IndexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Azure Search index");
            throw;
        }
    }

    public async Task IndexDocumentChunksAsync(List<DocumentChunk> chunks)
    {
        try
        {
            var documents = chunks.Select(chunk => new SearchDocument
            {
                ["id"] = chunk.Id,
                ["documentId"] = chunk.DocumentId,
                ["fileName"] = chunk.FileName,
                ["content"] = chunk.Content,
                ["chunkIndex"] = chunk.ChunkIndex,
                ["uploadedAt"] = chunk.UploadedAt
            }).ToList();

            var batch = IndexDocumentsBatch.Upload(documents);
            await _searchClient.IndexDocumentsAsync(batch);

            _logger.LogInformation("Indexed {Count} document chunks in Azure Search", chunks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing document chunks in Azure Search");
            throw;
        }
    }

    public async Task<List<SearchResult>> SearchTextAsync(string query, int topK)
    {
        try
        {
            var searchOptions = new SearchOptions
            {
                Size = topK,
                Select = { "id", "documentId", "fileName", "content", "chunkIndex" },
                SearchMode = SearchMode.Any,
                QueryType = SearchQueryType.Simple
            };

            var response = await _searchClient.SearchAsync<SearchDocument>(query, searchOptions);
            var results = new List<SearchResult>();

            await foreach (var result in response.Value.GetResultsAsync())
            {
                results.Add(new SearchResult
                {
                    Id = result.Document["id"].ToString() ?? string.Empty,
                    DocumentId = result.Document["documentId"].ToString() ?? string.Empty,
                    FileName = result.Document["fileName"].ToString() ?? string.Empty,
                    Content = result.Document["content"].ToString() ?? string.Empty,
                    Score = result.Score ?? 0
                });
            }

            _logger.LogInformation("Azure Search returned {Count} results for query: {Query}", results.Count, query);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing text search in Azure Search");
            throw;
        }
    }

    public async Task DeleteDocumentChunksAsync(string documentId)
    {
        try
        {
            var searchOptions = new SearchOptions
            {
                Filter = $"documentId eq '{documentId}'",
                Select = { "id" }
            };

            var response = await _searchClient.SearchAsync<SearchDocument>("*", searchOptions);
            var idsToDelete = new List<string>();

            await foreach (var result in response.Value.GetResultsAsync())
            {
                idsToDelete.Add(result.Document["id"].ToString() ?? string.Empty);
            }

            if (idsToDelete.Any())
            {
                var batch = IndexDocumentsBatch.Delete("id", idsToDelete);
                await _searchClient.IndexDocumentsAsync(batch);
                _logger.LogInformation("Deleted {Count} chunks for document {DocumentId} from Azure Search", idsToDelete.Count, documentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document chunks for {DocumentId} from Azure Search", documentId);
            throw;
        }
    }
}

public class SearchResult
{
    public string Id { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public double Score { get; set; }
}