namespace RagChatSystem.Api.Models;

public class QueryRequest
{
    public string Question { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
    public int TopK { get; set; } = 5;
}
