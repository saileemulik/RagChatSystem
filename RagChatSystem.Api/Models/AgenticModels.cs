namespace RagChatSystem.Api.Models;

public class AgenticQueryRequest
{
    public string Question { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
    public int TopK { get; set; } = 5;
    public bool UseWebSearch { get; set; } = true;
    public bool UseCalculator { get; set; } = true;
}

public class AgenticQueryResponse
{
    public string Answer { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public List<Citation> Citations { get; set; } = new();
    public List<ToolUsage> ToolsUsed { get; set; } = new();
    public int TokensUsed { get; set; }
}

public class ToolUsage
{
    public string ToolName { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}