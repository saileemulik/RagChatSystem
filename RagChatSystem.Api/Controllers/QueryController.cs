using Microsoft.AspNetCore.Mvc;
using RagChatSystem.Api.Models;
using RagChatSystem.Api.Services;

namespace RagChatSystem.Api.Controllers;

/// <summary>
/// Handles question answering queries using the RAG pattern
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class QueryController : ControllerBase
{
    private readonly IRagOrchestrationService _ragService;
    private readonly ILogger<QueryController> _logger;

    public QueryController(
        IRagOrchestrationService ragService,
        ILogger<QueryController> logger)
    {
        _ragService = ragService;
        _logger = logger;
    }

    /// <summary>
    /// Ask a question about ingested documents
    /// </summary>
    /// <param name="request">Query request containing the question and optional conversation ID</param>
    /// <returns>AI-generated answer with citations from source documents</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/query
    ///     Content-Type: application/json
    ///     
    ///     {
    ///       "question": "What are the main benefits of using Semantic Kernel?",
    ///       "topK": 5
    ///     }
    /// 
    /// For multi-turn conversations, include the conversationId from the previous response:
    /// 
    ///     {
    ///       "question": "Can you explain that in more detail?",
    ///       "conversationId": "guid-from-previous-response",
    ///       "topK": 5
    ///     }
    /// 
    /// This endpoint will:
    /// 1. Generate an embedding for the question
    /// 2. Perform vector search in Azure Cognitive Search
    /// 3. Retrieve the top-K most relevant document chunks
    /// 4. Build a prompt with the retrieved context
    /// 5. Generate an answer using Azure OpenAI
    /// 6. Return the answer with citations
    /// </remarks>
    /// <response code="200">Question answered successfully</response>
    /// <response code="400">Invalid question or request</response>
    /// <response code="500">Internal server error during query processing</response>
    [HttpPost]
    [ProducesResponseType(typeof(QueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Query([FromBody] QueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest("Question cannot be empty");
        }

        try
        {
            var response = await _ragService.QueryAsync(
                request.Question, 
                request.ConversationId, 
                request.TopK);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Query endpoint");
            return StatusCode(500, new 
            { 
                error = "Internal server error",
                message = ex.Message 
            });
        }
    }
}
