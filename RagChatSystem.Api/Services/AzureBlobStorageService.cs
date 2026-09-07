using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using RagChatSystem.Api.Models;

namespace RagChatSystem.Api.Services;

public interface IAzureBlobStorageService
{
    Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string documentId);
    Task<string> UploadPdfAsync(Stream fileStream, string fileName, string documentId);
    Task<Stream> DownloadDocumentAsync(string blobName);
    Task<Stream> DownloadPdfAsync(string blobName);
    Task DeleteDocumentAsync(string blobName);
    Task DeletePdfAsync(string blobName);
    Task<List<string>> GetAllBlobNamesAsync();
}

public class AzureBlobStorageService : IAzureBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(
        IConfiguration configuration,
        ILogger<AzureBlobStorageService> logger)
    {
        var settings = configuration.GetSection("AzureBlobStorage").Get<AzureBlobStorageSettings>()
            ?? throw new InvalidOperationException("AzureBlobStorage configuration is missing");

        var blobServiceClient = new BlobServiceClient(settings.ConnectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName);
        _logger = logger;
    }

    public async Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string documentId)
    {
        try
        {
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
            
            var blobName = $"{documentId}/{fileName}";
            var blobClient = _containerClient.GetBlobClient(blobName);

            fileStream.Position = 0;
            await blobClient.UploadAsync(fileStream, overwrite: true);

            _logger.LogInformation("Uploaded document {FileName} to Azure Blob Storage as {BlobName}", fileName, blobName);
            return blobName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document {FileName} to Azure Blob Storage", fileName);
            throw;
        }
    }

    public async Task<string> UploadPdfAsync(Stream fileStream, string fileName, string documentId)
    {
        return await UploadDocumentAsync(fileStream, fileName, documentId);
    }

    public async Task<Stream> DownloadDocumentAsync(string blobName)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            var downloadResponse = await blobClient.DownloadAsync();
            
            var memoryStream = new MemoryStream();
            await downloadResponse.Value.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {BlobName} from Azure Blob Storage", blobName);
            throw;
        }
    }

    public async Task<Stream> DownloadPdfAsync(string blobName)
    {
        return await DownloadDocumentAsync(blobName);
    }

    public async Task DeleteDocumentAsync(string blobName)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
            
            _logger.LogInformation("Deleted document {BlobName} from Azure Blob Storage", blobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {BlobName} from Azure Blob Storage", blobName);
            throw;
        }
    }

    public async Task DeletePdfAsync(string blobName)
    {
        await DeleteDocumentAsync(blobName);
    }

    public async Task<List<string>> GetAllBlobNamesAsync()
    {
        try
        {
            var blobNames = new List<string>();
            
            await foreach (var blobItem in _containerClient.GetBlobsAsync())
            {
                blobNames.Add(blobItem.Name);
            }
            
            return blobNames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blob names from Azure Blob Storage");
            throw;
        }
    }
}