namespace QuizPlatform.Application.DTOs.Exam;

/// <summary>
/// DTO for exam history item.
/// Represents a completed exam submission with key information.
/// </summary>
public class ExamHistoryDto
{
    public string SubmissionId { get; set; } = null!;
    public string ExamId { get; set; } = null!;
    public string ExamTitle { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? UserEmail { get; set; }
    public decimal TotalScore { get; set; }
    public decimal MaxScore { get; set; }
    public double Percentage { get; set; }
    public int TimeTaken { get; set; } // in seconds
    public int Duration { get; set; } // exam duration in minutes
    public DateTime SubmittedAt { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
}

/// <summary>
/// DTO for exam history list response.
/// </summary>
public class ExamHistoryListDto
{
    public List<ExamHistoryDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
