using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPlatform.Application.DTOs.Exam;
using QuizPlatform.Application.Interfaces;

namespace QuizPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExamController : ControllerBase
{
    private readonly IExamService _examService;

    public ExamController(IExamService examService)
    {
        _examService = examService;
    }

    private string GetUserId() => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

    /// <summary>
    /// Get all active exams for users.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExamListDto>>> GetActiveExams()
    {
        var exams = await _examService.GetActiveExamsAsync();
        return Ok(exams);
    }

    /// <summary>
    /// Start an exam attempt.
    /// </summary>
    [HttpPost("{examId}/start")]
    public async Task<ActionResult<StartExamResponse>> StartExam(string examId)
    {
        try
        {
            var result = await _examService.StartExamAsync(GetUserId(), examId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Save exam progress.
    /// </summary>
    [HttpPut("attempt/{attemptId}/save")]
    public async Task<ActionResult> SaveProgress(string attemptId, [FromBody] SaveProgressRequest request)
    {
        try
        {
            await _examService.SaveProgressAsync(attemptId, request);
            return Ok(new { message = "Progress saved" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Submit exam for grading.
    /// </summary>
    [HttpPost("attempt/{attemptId}/submit")]
    public async Task<ActionResult<ExamResultDto>> SubmitExam(string attemptId, [FromBody] SubmitExamRequest request)
    {
        try
        {
            var result = await _examService.SubmitExamAsync(GetUserId(), attemptId, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get exam result by submission ID.
    /// </summary>
    [HttpGet("result/{submissionId}")]
    public async Task<ActionResult<ExamResultDto>> GetResult(string submissionId)
    {
        var result = await _examService.GetResultAsync(submissionId);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    /// <summary>
    /// Get user's exam history.
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<ExamResultDto>>> GetHistory()
    {
        var history = await _examService.GetUserHistoryAsync(GetUserId());
        return Ok(history);
    }
}
