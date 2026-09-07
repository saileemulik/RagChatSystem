namespace RagChatSystem.Api.Models;

public class AzureOpenAISettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    public string EmbeddingDeploymentName { get; set; } = string.Empty;
}

public class AzureSearchSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
}

public class AzureBlobStorageSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}

public class DocumentProcessingSettings
{
    public int ChunkSize { get; set; } = 1000;
    public int ChunkOverlap { get; set; } = 200;
    public int TopK { get; set; } = 5;
}

public class AgenticAISettings
{
    public string BingSearchApiKey { get; set; } = string.Empty;
    public bool EnableWebSearch { get; set; } = true;
    public bool EnableCalculator { get; set; } = true;
    public bool EnableLiveData { get; set; } = true;
    public int MaxToolCalls { get; set; } = 5;
}
