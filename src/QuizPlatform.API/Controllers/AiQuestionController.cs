using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPlatform.Application.DTOs.Admin;
using QuizPlatform.Application.Interfaces;

namespace QuizPlatform.API.Controllers;

/// <summary>
/// Controller for AI-powered operations.
/// </summary>
[ApiController]
[Route("api/ai-question")]
[Authorize]
public class AiQuestionController : ControllerBase
{
    private readonly IAiQuestionService _aiQuestionService;
    private readonly IDocumentParserService _documentParser;

    public AiQuestionController(IAiQuestionService aiQuestionService, IDocumentParserService documentParser)
    {
        _aiQuestionService = aiQuestionService;
        _documentParser = documentParser;
    }

    /// <summary>
    /// Generates questions from uploaded file and/or user prompt.
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<IEnumerable<QuestionRequest>>> GenerateQuestions([FromForm] GenerateQuestionRequest request)
    {
        try
        {
            string? documentText = null;

            // 1. Parse File if provided
            if (request.File != null && request.File.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".docx", ".txt" };
                var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { message = "Chỉ hỗ trợ file PDF, DOCX hoặc TXT" });
                }

                using var stream = request.File.OpenReadStream();
                documentText = await _documentParser.ParseDocumentAsync(stream, request.File.FileName);
            }

            // 2. Validate Inputs
            if (string.IsNullOrWhiteSpace(documentText) && string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { message = "Vui lòng cung cấp file hoặc nội dung yêu cầu" });
            }

            // 3. Call AI Service
            var questions = await _aiQuestionService.GenerateQuestionsAsync(documentText, request.Prompt);
            return Ok(new { questions });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Lỗi tạo câu hỏi: {ex.Message}" });
        }
    }
}

public class GenerateQuestionRequest
{
    public IFormFile? File { get; set; }
    public string? Prompt { get; set; }
}
