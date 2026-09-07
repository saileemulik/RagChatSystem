using Microsoft.AspNetCore.Mvc;
using RagChatSystem.Api.Models;
using RagChatSystem.Api.Services;

namespace RagChatSystem.Api.Controllers;

/// <summary>
/// Handles document management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DocumentsController : ControllerBase
{
    private readonly IRagOrchestrationService _ragService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IRagOrchestrationService ragService,
        ILogger<DocumentsController> logger)
    {
        _ragService = ragService;
        _logger = logger;
    }

    /// <summary>
    /// Get list of all uploaded documents
    /// </summary>
    /// <returns>List of documents with metadata</returns>
    [HttpGet]
    [ProducesResponseType(typeof(DocumentListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDocuments()
    {
        try
        {
            var response = await _ragService.GetDocumentListAsync();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents list");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a document by ID
    /// </summary>
    /// <param name="documentId">Document ID to delete</param>
    /// <returns>Success status</returns>
    [HttpDelete("{documentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}