using QuizPlatform.Domain.Common;
using QuizPlatform.Domain.ValueObjects;

namespace QuizPlatform.Domain.Entities;

/// <summary>
/// Represents a submitted exam with graded answers.
/// </summary>
public class Submission : BaseEntity
{
    public string UserId { get; set; } = null!;
    public string ExamId { get; set; } = null!;
    public List<GradedAnswer> Answers { get; set; } = new();
    public decimal TotalScore { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public int TimeTaken { get; set; } // in seconds
}
