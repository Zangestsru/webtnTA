using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPlatform.Application.DTOs.Admin;
using QuizPlatform.Application.Interfaces;

namespace QuizPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Teacher")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    private string GetUserId() => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

    #region Question Endpoints

    /// <summary>
    /// Get all questions (admin).
    /// </summary>
    [HttpGet("questions")]
    public async Task<ActionResult<IEnumerable<AdminQuestionDto>>> GetAllQuestions()
    {
        var questions = await _adminService.GetAllQuestionsAsync();
        return Ok(questions);
    }

    /// <summary>
    /// Get question by ID (admin).
    /// </summary>
    [HttpGet("questions/{id}")]
    public async Task<ActionResult<AdminQuestionDto>> GetQuestion(string id)
    {
        var question = await _adminService.GetQuestionByIdAsync(id);
        if (question == null)
        {
            return NotFound();
        }
        return Ok(question);
    }

    /// <summary>
    /// Create a new question (admin).
    /// </summary>
    [HttpPost("questions")]
    public async Task<ActionResult<AdminQuestionDto>> CreateQuestion([FromBody] QuestionRequest request)
    {
        var question = await _adminService.CreateQuestionAsync(request);
        return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, question);
    }

    /// <summary>
    /// Update a question (admin).
    /// </summary>
    [HttpPut("questions/{id}")]
    public async Task<ActionResult<AdminQuestionDto>> UpdateQuestion(string id, [FromBody] QuestionRequest request)
    {
        try
        {
            var question = await _adminService.UpdateQuestionAsync(id, request);
            return Ok(question);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a question (admin).
    /// </summary>
    [HttpDelete("questions/{id}")]
    public async Task<ActionResult> DeleteQuestion(string id)
    {
        await _adminService.DeleteQuestionAsync(id);
        return NoContent();
    }

    #endregion

    #region Exam Endpoints

    /// <summary>
    /// Get all exams (admin).
    /// </summary>
    [HttpGet("exams")]
    public async Task<ActionResult<IEnumerable<AdminExamDto>>> GetAllExams()
    {
        var exams = await _adminService.GetAllExamsAsync();
        return Ok(exams);
    }

    /// <summary>
    /// Get exam by ID (admin).
    /// </summary>
    [HttpGet("exams/{id}")]
    public async Task<ActionResult<AdminExamDto>> GetExam(string id)
    {
        var exam = await _adminService.GetExamByIdAsync(id);
        if (exam == null)
        {
            return NotFound();
        }
        return Ok(exam);
    }

    /// <summary>
    /// Create a new exam (admin).
    /// </summary>
    [HttpPost("exams")]
    public async Task<ActionResult<AdminExamDto>> CreateExam([FromBody] ExamRequest request)
    {
        var exam = await _adminService.CreateExamAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetExam), new { id = exam.Id }, exam);
    }

    /// <summary>
    /// Update an exam (admin).
    /// </summary>
    [HttpPut("exams/{id}")]
    public async Task<ActionResult<AdminExamDto>> UpdateExam(string id, [FromBody] ExamRequest request)
    {
        try
        {
            var exam = await _adminService.UpdateExamAsync(id, request);
            return Ok(exam);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete an exam (admin).
    /// </summary>
    [HttpDelete("exams/{id}")]
    public async Task<ActionResult> DeleteExam(string id)
    {
        await _adminService.DeleteExamAsync(id);
        return NoContent();
    }

    #endregion

    #region User Endpoints

    /// <summary>
    /// Get all users (admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<AdminUserDto>>> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Update user role (admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("users/{id}/role")]
    public async Task<ActionResult<AdminUserDto>> UpdateUserRole(string id, [FromBody] UpdateRoleRequest request)
    {
        try
        {
            var user = await _adminService.UpdateUserRoleAsync(id, request);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Toggle user active status (admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("users/{id}/toggle-active")]
    public async Task<ActionResult<AdminUserDto>> ToggleUserActive(string id)
    {
        try
        {
            var user = await _adminService.ToggleUserActiveAsync(id);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a user (admin).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("users/{id}")]
    public async Task<ActionResult> DeleteUser(string id)
    {
        try
        {
            await _adminService.DeleteUserAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    #endregion
}
