namespace RagChatSystem.Api.Models;

public class DocumentInfo
{
    public string DocumentId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

public class DocumentListResponse
{
    public List<DocumentInfo> Documents { get; set; } = new();
    public int TotalCount { get; set; }
}