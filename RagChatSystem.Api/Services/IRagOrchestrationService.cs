using RagChatSystem.Api.Models;

namespace RagChatSystem.Api.Services;

public interface IRagOrchestrationService
{
    Task<IngestResponse> IngestDocumentAsync(Stream fileStream, string fileName, Dictionary<string, string> metadata);
    Task<QueryResponse> QueryAsync(string question, string? conversationId, int topK);
    Task DeleteDocumentAsync(string documentId);
    Task<List<IngestResponse>> GetDocumentsAsync();
    Task<DocumentListResponse> GetDocumentListAsync();
}