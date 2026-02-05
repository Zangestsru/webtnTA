using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPlatform.Application.Interfaces;

namespace QuizPlatform.API.Controllers;

/// <summary>
/// Controller for exam history operations.
/// Provides endpoints for viewing completed exam submissions.
/// </summary>
[ApiController]
[Route("api/exam-history")]
[Authorize]
public class ExamHistoryController : ControllerBase
{
    private readonly IExamHistoryService _examHistoryService;

    public ExamHistoryController(IExamHistoryService examHistoryService)
    {
        _examHistoryService = examHistoryService;
    }

    /// <summary>
    /// Get current user's exam history.
    /// </summary>
    [HttpGet("my-history")]
    public async Task<IActionResult> GetMyHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _examHistoryService.GetUserHistoryAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get all exam history (Admin only).
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllHistory(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20,
        [FromQuery] string? examId = null,
        [FromQuery] string? userId = null)
    {
        var result = await _examHistoryService.GetAllHistoryAsync(page, pageSize, examId, userId);
        return Ok(result);
    }

    /// <summary>
    /// Get specific user's exam history (Admin only).
    /// </summary>
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserHistory(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _examHistoryService.GetUserHistoryAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get submission details by ID.
    /// Users can only view their own submissions.
    /// </summary>
    [HttpGet("submission/{submissionId}")]
    public async Task<IActionResult> GetSubmissionDetails(string submissionId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        // Admin can view any submission, users can only view their own
        var result = userRole == "Admin" 
            ? await _examHistoryService.GetSubmissionDetailsAsync(submissionId)
            : await _examHistoryService.GetSubmissionDetailsAsync(submissionId, userId);

        if (result == null)
        {
            return NotFound(new { message = "Submission not found or access denied" });
        }

        return Ok(result);
    }
}
