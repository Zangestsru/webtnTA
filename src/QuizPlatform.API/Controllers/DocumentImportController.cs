using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPlatform.Application.DTOs.Admin;
using QuizPlatform.Application.Interfaces;

namespace QuizPlatform.API.Controllers;

/// <summary>
/// Controller for importing questions from documents using AI.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DocumentImportController : ControllerBase
{
    private readonly IDocumentParserService _documentParser;
    private readonly IAiQuestionService _aiQuestionService;
    private readonly IAdminService _adminService;

    public DocumentImportController(
        IDocumentParserService documentParser,
        IAiQuestionService aiQuestionService,
        IAdminService adminService)
    {
        _documentParser = documentParser;
        _aiQuestionService = aiQuestionService;
        _adminService = adminService;
    }

    /// <summary>
    /// Upload a document and extract questions using AI.
    /// </summary>
    [HttpPost("parse")]
    public async Task<ActionResult<IEnumerable<QuestionRequest>>> ParseDocument(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded" });
        }

        var allowedExtensions = new[] { ".pdf", ".docx" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Only PDF and DOCX files are supported" });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var documentText = await _documentParser.ParseDocumentAsync(stream, file.FileName);

            if (string.IsNullOrWhiteSpace(documentText))
            {
                return BadRequest(new { message = "Could not extract text from document" });
            }

            var questions = await _aiQuestionService.ExtractQuestionsAsync(documentText);
            return Ok(questions);
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = $"AI parsing error: {ex.Message}" });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new { message = $"AI service unavailable: {ex.Message}" });
        }
    }

    /// <summary>
    /// Confirm and save extracted questions to database.
    /// </summary>
    [HttpPost("confirm")]
    public async Task<ActionResult<IEnumerable<AdminQuestionDto>>> ConfirmQuestions([FromBody] List<QuestionRequest> questions)
    {
        if (questions == null || questions.Count == 0)
        {
            return BadRequest(new { message = "No questions provided" });
        }

        var savedQuestions = new List<AdminQuestionDto>();

        foreach (var question in questions)
        {
            try
            {
                var saved = await _adminService.CreateQuestionAsync(question);
                savedQuestions.Add(saved);
            }
            catch (Exception ex)
            {
                // Log but continue with other questions
                Console.WriteLine($"Failed to save question: {ex.Message}");
            }
        }

        return Ok(new
        {
            saved = savedQuestions.Count,
            total = questions.Count,
            questions = savedQuestions
        });
    }
}
