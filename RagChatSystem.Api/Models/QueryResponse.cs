namespace RagChatSystem.Api.Models;

public class QueryResponse
{
    public string Answer { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public List<Citation> Citations { get; set; } = new();
    public int TokensUsed { get; set; }
}

public class Citation
{
    public string FileName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public double Score { get; set; }
}
