using Microsoft.AspNetCore.Mvc;
using RagChatSystem.Api.Models;
using RagChatSystem.Api.Services;

namespace RagChatSystem.Api.Controllers;

/// <summary>
/// Handles document ingestion for the RAG system (PDF, DOCX, Excel, TXT, CSV)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class IngestController : ControllerBase
{
    private readonly IRagOrchestrationService _ragService;
    private readonly ILogger<IngestController> _logger;

    public IngestController(
        IRagOrchestrationService ragService,
        ILogger<IngestController> logger)
    {
        _ragService = ragService;
        _logger = logger;
    }

    /// <summary>
    /// Get all uploaded documents
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDocuments()
    {
        try
        {
            var documents = await _ragService.GetDocumentsAsync();
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents");
            return StatusCode(500, new { message = $"Error: {ex.Message}" });
        }
    }

    /// <summary>
    /// Upload and ingest a document into the RAG system
    /// </summary>
    /// <param name="file">Document file to upload (PDF, DOCX, XLSX, XLS, TXT, CSV)</param>
    /// <param name="metadata">Optional metadata in key=value format separated by semicolons (e.g., author=John Doe;category=Technical)</param>
    /// <returns>Ingestion status with document ID and chunk count</returns>
    [HttpPost]
    [ProducesResponseType(typeof(IngestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IngestDocument([FromForm] IFormFile file, [FromForm] string? metadata)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var supportedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".xls", ".txt", ".csv" };
        if (!supportedExtensions.Contains(extension))
        {
            return BadRequest("Supported file formats: PDF, DOCX, XLSX, XLS, TXT, CSV");
        }

        try
        {
            var metadataDict = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(metadata))
            {
                // Simple key=value parsing
                var pairs = metadata.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var pair in pairs)
                {
                    var kv = pair.Split('=', 2);
                    if (kv.Length == 2)
                    {
                        metadataDict[kv[0].Trim()] = kv[1].Trim();
                    }
                }
            }

            using var stream = file.OpenReadStream();
            var response = await _ragService.IngestDocumentAsync(stream, file.FileName, metadataDict);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in IngestDocument endpoint");
            return StatusCode(500, new IngestResponse
            {
                Status = "Failed",
                Message = $"Internal server error: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Delete a document from the RAG system
    /// </summary>
    /// <param name="documentId">Document ID to delete</param>
    [HttpDelete("{documentId}")]
    public async Task<IActionResult> DeleteDocument(string documentId)
    {
        try
        {
            await _ragService.DeleteDocumentAsync(documentId);
            return Ok(new { message = "Document deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            return StatusCode(500, new { message = $"Error: {ex.Message}" });
        }
    }
}