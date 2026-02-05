using QuizPlatform.Domain.Entities;
using QuizPlatform.Domain.ValueObjects;

namespace QuizPlatform.Application.DTOs.Exam;

/// <summary>
/// DTO for exam list display.
/// </summary>
public class ExamListDto
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Duration { get; set; }
    public decimal TotalScore { get; set; }
    public int QuestionCount { get; set; }
}

/// <summary>
/// DTO for exam details.
/// </summary>
public class ExamDetailDto : ExamListDto
{
    public List<QuestionDto> Questions { get; set; } = new();
}

/// <summary>
/// DTO for question display (without correct answers for users).
/// </summary>
public class QuestionDto
{
    public string Id { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Type { get; set; } = null!;
    public List<AnswerOption> Options { get; set; } = new();
    public decimal Score { get; set; }
    public int Order { get; set; }
    public string? AudioUrl { get; set; }
    public string? ImageUrl { get; set; }
}

/// <summary>
/// DTO for starting an exam.
/// </summary>
public class StartExamResponse
{
    public string AttemptId { get; set; } = null!;
    public string ExamId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public int Duration { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime ExpiredAt { get; set; }
    public List<QuestionDto> Questions { get; set; } = new();
}

/// <summary>
/// DTO for saving exam progress.
/// </summary>
public class SaveProgressRequest
{
    public List<AttemptAnswer> Answers { get; set; } = new();
}

/// <summary>
/// DTO for submitting exam.
/// </summary>
public class SubmitExamRequest
{
    public List<SubmitAnswer> Answers { get; set; } = new();
}

/// <summary>
/// Single answer in submission.
/// </summary>
public class SubmitAnswer
{
    public string QuestionId { get; set; } = null!;
    public List<string> SelectedAnswers { get; set; } = new();
}

/// <summary>
/// DTO for exam result.
/// </summary>
public class ExamResultDto
{
    public string SubmissionId { get; set; } = null!;
    public string ExamTitle { get; set; } = null!;
    public decimal TotalScore { get; set; }
    public decimal MaxScore { get; set; }
    public int TimeTaken { get; set; }
    public DateTime SubmittedAt { get; set; }
    public List<QuestionResultDto> Questions { get; set; } = new();
}

/// <summary>
/// DTO for question result with explanation.
/// </summary>
public class QuestionResultDto
{
    public string Id { get; set; } = null!;
    public string Content { get; set; } = null!;
    public List<AnswerOption> Options { get; set; } = new();
    public List<string> CorrectAnswers { get; set; } = new();
    public List<string> UserAnswers { get; set; } = new();
    public bool IsCorrect { get; set; }
    public decimal Score { get; set; }
    public string? Explanation { get; set; }
}

/// <summary>
/// DTO for submission details with full question information.
/// </summary>
public class SubmissionDto
{
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? UserEmail { get; set; }
    public string ExamId { get; set; } = null!;
    public string ExamTitle { get; set; } = null!;
    public decimal TotalScore { get; set; }
    public decimal MaxScore { get; set; }
    public List<QuestionResultDto> Questions { get; set; } = new();
    public DateTime SubmittedAt { get; set; }
    public int TimeTaken { get; set; }
}
