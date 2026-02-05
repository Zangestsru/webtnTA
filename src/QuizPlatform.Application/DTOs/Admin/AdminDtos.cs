using QuizPlatform.Domain.Entities;
using QuizPlatform.Domain.ValueObjects;
using QuizPlatform.Domain.Enums;

namespace QuizPlatform.Application.DTOs.Admin;

/// <summary>
/// Request DTO for creating/updating a question.
/// </summary>
public class QuestionRequest
{
    public string? ExamId { get; set; }
    public string Content { get; set; } = null!;
    public string Type { get; set; } = "Single";
    public List<AnswerOption> Options { get; set; } = new();
    public List<string> CorrectAnswers { get; set; } = new();
    public string? Explanation { get; set; }
    public decimal Score { get; set; } = 1;
    public int Order { get; set; }
    public string? AudioUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public string Difficulty { get; set; } = "Medium";
}

/// <summary>
/// Request DTO for creating/updating an exam.
/// </summary>
public class ExamRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Duration { get; set; }
    public decimal TotalScore { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsRandom { get; set; }
    public int QuestionCount { get; set; }
    public List<string> QuestionIds { get; set; } = new(); // For manual question selection
    public List<string> Categories { get; set; } = new(); // For random selection by category
}

/// <summary>
/// DTO for admin question list.
/// </summary>
public class AdminQuestionDto
{
    public string Id { get; set; } = null!;
    public string? ExamId { get; set; }
    public string Content { get; set; } = null!;
    public string Type { get; set; } = null!;
    public List<AnswerOption> Options { get; set; } = new();
    public List<string> CorrectAnswers { get; set; } = new();
    public string? Explanation { get; set; }
    public decimal Score { get; set; }
    public string? Category { get; set; }
    public string Difficulty { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for admin exam list.
/// </summary>
public class AdminExamDto
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Duration { get; set; }
    public decimal TotalScore { get; set; }
    public bool IsActive { get; set; }
    public bool IsRandom { get; set; }
    public int QuestionCount { get; set; }
    public List<string> QuestionIds { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for admin user list.
/// </summary>
public class AdminUserDto
{
    public string Id { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request for updating user role.
/// </summary>
public class UpdateRoleRequest
{
    public string Role { get; set; } = null!;
}
