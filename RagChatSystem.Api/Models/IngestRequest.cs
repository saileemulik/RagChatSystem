namespace RagChatSystem.Api.Models;

public class IngestRequest
{
    public IFormFile? File { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
